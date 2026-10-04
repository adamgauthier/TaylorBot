using Dapper;
using System.Globalization;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Scenarios;

public sealed partial class ScenarioData
{
    public Task<ScenarioGuild> EventGuildAsync(ScenarioUser user, params ScenarioRole[] roles) =>
        GuildAsync(user, roles: roles, id: "115332333745340416");

    public async Task CouponAsync(string code = "ENCHANTED", long reward = 50, int limit = 2, int validDays = 1, int used = 0)
    {
        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync("""
            INSERT INTO commands.coupons (code, valid_from, valid_until, usage_limit, used_count, taypoint_reward)
            VALUES (@code, CURRENT_TIMESTAMP - interval '2 days', CURRENT_TIMESTAMP + @validDays * interval '1 day', @limit, @used, @reward);
            """, new { code, reward, limit, validDays, used });
    }

    public async Task LoveEventAsync(ScenarioGuild guild, ScenarioRole role, bool ended = false)
    {
        Dictionary<string, string> values = new()
        {
            ["spread_love_role_id"] = role.Id,
            ["incubation_period"] = "01:00:00",
            ["spread_limit"] = "3",
            ["lounge_channel_id"] = guild.ChannelId,
            ["giveaways_end_time"] = DateTimeOffset.UtcNow.AddDays(ended ? -1 : 1).ToString("O", CultureInfo.InvariantCulture),
            ["spread_end_time"] = DateTimeOffset.UtcNow.AddDays(ended ? -1 : 1).ToString("O", CultureInfo.InvariantCulture),
            ["timespan_between_giveaways"] = "01:00:00",
            ["giveaway_prize_min"] = "10",
            ["giveaway_prize_max"] = "20",
        };
        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync("""
            INSERT INTO valentines2026.config (config_key, config_value) VALUES (@Key, @Value)
            ON CONFLICT (config_key) DO UPDATE SET config_value = excluded.config_value;
            """, values.Select(pair => new { pair.Key, pair.Value }));
    }

    public async Task LoveReceivedAsync(ScenarioUser user, ScenarioUser from, int hoursAgo = 2)
    {
        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync("""
            INSERT INTO valentines2026.role_obtained (user_id, username, acquired_from_user_id, acquired_from_username, acquired_at)
            VALUES (@Id, @Username, @FromId, @FromName, CURRENT_TIMESTAMP - @hoursAgo * interval '1 hour');
            """, new { user.Id, user.Username, FromId = from.Id, FromName = from.Username, hoursAgo });
    }

    public async Task JoinedBeforeAnniversaryAsync(ScenarioGuild guild, ScenarioUser user)
    {
        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync("""
            UPDATE guilds.guild_members SET first_joined_at = '2024-01-01T00:00:00Z'
            WHERE guild_id = @GuildId AND user_id = @Id;
            """, new { GuildId = guild.Id, user.Id });
    }

    public async Task RecapAsync(ScenarioUser user, byte[]? image = null)
    {
        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync("""
            INSERT INTO configuration.application_info (info_key, info_value) VALUES ('recap_2025_count', '500')
            ON CONFLICT (info_key) DO UPDATE SET info_value = excluded.info_value;
            """);
        if (image != null)
        {
            await connection.ExecuteAsync("""
                INSERT INTO configuration.application_info (info_key, info_value) VALUES (@Key, @Value)
                ON CONFLICT (info_key) DO UPDATE SET info_value = excluded.info_value;
                """, new { Key = $"recap_2025_{user.Id}.jpg.base64", Value = Convert.ToBase64String(image) });
        }
    }
}
