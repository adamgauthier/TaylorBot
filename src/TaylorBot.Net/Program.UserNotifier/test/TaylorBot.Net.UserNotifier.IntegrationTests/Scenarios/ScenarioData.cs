using Dapper;
using TaylorBot.Net.UserNotifier.IntegrationTests.Discord;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.Scenarios;

public sealed record ScenarioUser(string Id, string Username);

public sealed record ScenarioGuild(string Id, string ChannelId, ScenarioUser Owner)
{
    public IReadOnlyDictionary<string, string> Channels { get; init; } = new Dictionary<string, string>();
}

public sealed record ScenarioChannel(string Name, bool EveryoneCanSend = true, bool BotCanSend = true, int Type = 0);

public sealed partial class ScenarioData(ScenarioDatabase database, NotifierDiscordApi api, Func<string, object, Task> dispatch)
{
    private long _id = 100000000000000100;

    public ScenarioUser Bot { get; } = new(NotifierDiscordApi.BotId, "IntegrationBot");

    public async Task<ScenarioUser> UserAsync(string username = "Alice", long taypoints = 0)
    {
        ScenarioUser user = new($"{Interlocked.Increment(ref _id)}", username);

        await using var connection = database.CreateConnection();
        await connection.ExecuteAsync(
            "INSERT INTO users.users (user_id, username, is_bot, taypoint_count) VALUES (@Id, @Username, false, @taypoints);",
            new { user.Id, user.Username, taypoints });

        api.Resource($"users/{user.Id}", NotifierDiscordApi.User(user.Id, user.Username));

        return user;
    }

    public async Task<ScenarioGuild> GuildAsync(ScenarioUser owner, string name = "Integration guild",
        IReadOnlyList<ScenarioChannel>? channels = null, string? welcomeChannel = "general")
    {
        ScenarioGuild guild = new($"{Interlocked.Increment(ref _id)}", $"{Interlocked.Increment(ref _id)}", owner);
        channels ??= [new("general")];
        var channelIds = channels.Select((channel, index) => (channel.Name, Id: index == 0 ? guild.ChannelId : $"{Interlocked.Increment(ref _id)}")).ToDictionary(channel => channel.Name, channel => channel.Id);
        guild = guild with { Channels = channelIds };
        var channelPayloads = channels.Select(channel => Channel(guild with { ChannelId = channelIds[channel.Name] }, channel)).ToArray();
        var members = new[] { Member(owner), Member(new(NotifierDiscordApi.BotId, "IntegrationBot")) };

        foreach (var channel in channels)
        {
            api.Resource($"channels/{channelIds[channel.Name]}", Channel(guild with { ChannelId = channelIds[channel.Name] }, channel));
        }

        api.Resource($"guilds/{guild.Id}/members/{owner.Id}", members[0]);
        api.Resource($"guilds/{guild.Id}/members/{NotifierDiscordApi.BotId}", members[1]);
        api.Members(guild.Id, members);

        if (welcomeChannel != null)
        {
            api.ExpectMessage(channelIds[welcomeChannel]);
        }

        var payload = new
        {
            id = guild.Id,
            name,
            owner_id = owner.Id,
            icon = (string?)null,
            afk_channel_id = (string?)null,
            afk_timeout = 300,
            verification_level = 0,
            default_message_notifications = 0,
            explicit_content_filter = 0,
            mfa_level = 0,
            premium_tier = 0,
            premium_subscription_count = 0,
            preferred_locale = "en-US",
            nsfw_level = 0,
            features = Array.Empty<string>(),
            emojis = Array.Empty<object>(),
            stickers = Array.Empty<object>(),
            roles = new[] { new { id = guild.Id, name = "@everyone", color = 0, hoist = false, position = 0, permissions = "68608", managed = false, mentionable = false, flags = 0 } },
            channels = channelPayloads,
            members,
            member_count = members.Length,
            large = false,
            joined_at = "2026-01-01T00:00:00Z",
            presences = Array.Empty<object>(),
            voice_states = Array.Empty<object>(),
            threads = Array.Empty<object>(),
            stage_instances = Array.Empty<object>(),
            guild_scheduled_events = Array.Empty<object>(),
        };

        api.Resource($"guilds/{guild.Id}", payload);

        await dispatch("GUILD_CREATE", payload);

        return guild;
    }

    internal static object Channel(ScenarioGuild guild, ScenarioChannel channel) => new
    {
        id = guild.ChannelId,
        guild_id = guild.Id,
        type = channel.Type,
        name = channel.Name,
        position = 0,
        permission_overwrites = new[]
        {
            new { id = guild.Id, type = 0, allow = "0", deny = channel.EveryoneCanSend ? "0" : "2048" },
            new { id = NotifierDiscordApi.BotId, type = 1, allow = "0", deny = channel.BotCanSend ? "0" : "2048" },
        },
    };

    internal static object Member(ScenarioUser user, IReadOnlyList<string>? roles = null) => new
    {
        user = NotifierDiscordApi.User(user.Id, user.Username, bot: user.Id == NotifierDiscordApi.BotId),
        roles = roles ?? [],
        joined_at = "2026-01-01T00:00:00Z",
        deaf = false,
        mute = false,
        flags = 0,
    };

    public async Task SpamChannelAsync(ScenarioGuild guild)
    {
        await using var connection = database.CreateConnection();
        await connection.ExecuteAsync("UPDATE guilds.text_channels SET is_spam = true WHERE channel_id = @ChannelId;", new { guild.ChannelId });

        await database.Redis.KeyDeleteAsync($"spam-channel:guild:{guild.Id}:channel:{guild.ChannelId}");
    }

    public async Task<Guid> ReminderAsync(ScenarioUser user, string text = "Remember the tea", bool due = true)
    {
        var id = Guid.NewGuid();
        await using var connection = database.CreateConnection();
        await connection.ExecuteAsync(
            """
            INSERT INTO users.reminders (reminder_id, user_id, reminder_text, created_at, remind_at)
            VALUES (@ReminderId, @Id, @text, CURRENT_TIMESTAMP - interval '1 day', CURRENT_TIMESTAMP + @delay);
            """, new { ReminderId = id, user.Id, text, delay = due ? -TimeSpan.FromMinutes(1) : TimeSpan.FromDays(1) });

        return id;
    }

    public async Task ActiveMemberAsync(ScenarioGuild guild, ScenarioUser user, bool active = true)
    {
        await using var connection = database.CreateConnection();
        await connection.ExecuteAsync(
            "UPDATE guilds.guild_members SET last_spoke_at = CURRENT_TIMESTAMP - @age WHERE guild_id = @GuildId AND user_id = @Id;",
            new { GuildId = guild.Id, user.Id, age = active ? TimeSpan.Zero : TimeSpan.FromDays(1) });
    }
}
