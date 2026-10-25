using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Npgsql;
using TaylorBot.Net.IntegrationTests.Shared.Infrastructure;
using TaylorBot.Net.UserNotifier.LoadTests;

namespace TaylorBot.Net.UserNotifier.LoadController;

internal static class Program
{
    private static readonly JsonSerializerOptions ProfileJson = new() { UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow };

    public static async Task<int> Main(string[] args)
    {
        if (args is ["verify"])
        {
            using HttpClient client = new() { BaseAddress = new("http://localhost:8080") };
            using var response = await client.PostAsync("verify", content: null);
            Console.WriteLine(await response.Content.ReadAsStringAsync());
            return response.IsSuccessStatusCode ? 0 : 1;
        }

        var profile = JsonSerializer.Deserialize<LoadProfile>(await File.ReadAllTextAsync("/inputs/profile.json"), ProfileJson)
            ?? throw new InvalidOperationException("Missing profile.");
        profile.Validate();
        await File.WriteAllTextAsync("/results/profile.json", JsonSerializer.Serialize(profile));
        await using DataServices services = new();
        await services.InitializeAsync();
        await using var database = await services.CreateDatabaseAsync();
        await SeedAsync(database, profile);
        var builder = WebApplication.CreateBuilder();
        builder.Logging.SetMinimumLevel(LogLevel.Warning);
        await using var app = builder.Build();
        using Gateway gateway = new(profile, database, app.Logger);
        app.UseWebSockets();
        app.Map("/gateway", gateway.ConnectAsync);
        app.MapGet("/configuration", () => Configuration(services, database, profile));
        app.MapPost("/samples", async (WorkerSample sample) =>
        {
            var status = await gateway.SampleAsync(sample);
            if (status.Finished)
            {
                await File.WriteAllTextAsync("/results/status.json", JsonSerializer.Serialize(status));
            }
            return status;
        });
        app.Map("/api/{**path}", gateway.RestAsync);
        app.MapPost("/verify", async () =>
        {
            var result = await gateway.VerifyAsync();
            await File.WriteAllTextAsync("/results/verification.json", JsonSerializer.Serialize(result));
            return result.Success ? Results.Ok(result) : Results.Json(result, statusCode: 500);
        });
        await app.StartAsync();
        await File.WriteAllTextAsync("/results/controller-ready", "ready");
        await app.WaitForShutdownAsync();
        return 0;
    }

    private static Dictionary<string, string?> Configuration(DataServices services, ScenarioDatabase database, LoadProfile profile)
    {
        NpgsqlConnectionStringBuilder connection = new(database.ConnectionString);
        return new()
        {
            ["APPLICATIONINSIGHTS_CONNECTION_STRING"] = "no_application_insights",
            ["Discord:Token"] = "MTAwMDAwMDAwMDAwMDAwMDAx.synthetic.synthetic-test-token-never-a-real-credential",
            ["Discord:ShardCount"] = $"{profile.Shards}",
            ["Discord:MessageCacheSize"] = $"{profile.MessageCacheSize}",
            ["Discord:StartupDelay"] = "00:00:00",
            ["DatabaseConnection:Host"] = connection.Host,
            ["DatabaseConnection:Port"] = $"{connection.Port}",
            ["DatabaseConnection:Username"] = connection.Username,
            ["DatabaseConnection:Password"] = connection.Password,
            ["DatabaseConnection:Database"] = connection.Database,
            ["RedisConnection:Host"] = services.RedisHost,
            ["RedisConnection:Port"] = $"{services.RedisPort}",
            ["RedisConnection:Password"] = "",
            ["EntityTracker:TimeSpanBetweenGuildProcessedInReady"] = TimeSpan.FromMilliseconds(profile.StartupGuildDelayMilliseconds).ToString(),
            ["PatreonSync:Enabled"] = "false",
            ["PatreonSync:CampaignId"] = "13",
            ["PatreonSync:ApiKey"] = "synthetic",
            ["RedditAuth:AppId"] = "synthetic",
            ["RedditAuth:AppSecret"] = "synthetic",
            ["YoutubeAuth:ApiKey"] = "synthetic",
            ["TumblrAuth:ConsumerKey"] = "synthetic",
            ["TumblrAuth:ConsumerSecret"] = "synthetic",
            ["TumblrAuth:Token"] = "synthetic",
            ["TumblrAuth:TokenSecret"] = "synthetic",
            ["Logging:LogLevel:Default"] = "Information",
        };
    }

    private static async Task SeedAsync(ScenarioDatabase database, LoadProfile profile)
    {
        await using var connection = database.CreateConnection();
        await connection.OpenAsync();
        await connection.ExecuteAsync("""
            CREATE TEMP TABLE synthetic_guilds AS
            SELECT i, ((24000000000::bigint + i) << 22)::text AS guild_id,
                (200000000000000000::bigint + (i % @UserPools)::bigint * 1000000 + 1)::text AS owner_id
            FROM generate_series(0, @Guilds - 1) i;
            INSERT INTO guilds.guilds (guild_id, guild_name)
            SELECT guild_id, 'Synthetic guild ' || i FROM synthetic_guilds;
            INSERT INTO users.users (user_id, username, is_bot)
            SELECT DISTINCT id, CASE WHEN id = @Bot THEN 'SyntheticBot' ELSE 'Synthetic' || id END, id = @Bot
            FROM synthetic_guilds CROSS JOIN generate_series(0, @CachedMembersPerGuild - 1) m
            CROSS JOIN LATERAL (SELECT CASE WHEN m = 0 THEN @Bot ELSE
                (200000000000000000::bigint + (i % @UserPools)::bigint * 1000000 + m)::text END AS id) u;
            INSERT INTO guilds.guild_members (guild_id, user_id)
            SELECT guild_id, CASE WHEN m = 0 THEN @Bot ELSE
                (200000000000000000::bigint + (i % @UserPools)::bigint * 1000000 + m)::text END
            FROM synthetic_guilds CROSS JOIN generate_series(0, @CachedMembersPerGuild - 1) m;
            INSERT INTO guilds.text_channels (guild_id, channel_id)
            SELECT guild_id, (300000000000000000::bigint + i::bigint * 1000 + c)::text
            FROM synthetic_guilds CROSS JOIN generate_series(0, @ChannelsPerGuild - 1) c;
            """, new { profile.UserPools, profile.Guilds, profile.CachedMembersPerGuild, profile.ChannelsPerGuild, Bot = Payloads.BotId },
            commandTimeout: 120);
        if (profile.ModerationLogging)
        {
            await connection.ExecuteAsync("""
                INSERT INTO plus.plus_users (user_id, active, max_plus_guilds, source)
                SELECT DISTINCT owner_id, true, @Guilds, 'manual_dont_reward' FROM synthetic_guilds WHERE i % 10 = 0;
                INSERT INTO plus.plus_guilds (guild_id, plus_user_id, state)
                SELECT guild_id, owner_id, 'enabled' FROM synthetic_guilds WHERE i % 10 = 0;
                INSERT INTO plus.deleted_log_channels (guild_id, deleted_log_channel_id)
                SELECT guild_id, (300000000000000000::bigint + i::bigint * 1000)::text FROM synthetic_guilds WHERE i % 10 = 0;
                INSERT INTO plus.edited_log_channels (guild_id, edited_log_channel_id)
                SELECT guild_id, (300000000000000000::bigint + i::bigint * 1000)::text FROM synthetic_guilds WHERE i % 10 = 0;
                INSERT INTO plus.member_log_channels (guild_id, member_log_channel_id)
                SELECT guild_id, (300000000000000000::bigint + i::bigint * 1000)::text FROM synthetic_guilds WHERE i % 10 = 0;
                """, new { profile.Guilds });
        }
    }
}
