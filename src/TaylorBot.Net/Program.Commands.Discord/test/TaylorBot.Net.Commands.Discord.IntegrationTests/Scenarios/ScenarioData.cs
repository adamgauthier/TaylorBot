using Dapper;
using System.Text.Json.Nodes;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Scenarios;

public sealed record ScenarioUser(string Id, string Username, string? Avatar = null);
public sealed record ScenarioDm(ScenarioUser Recipient, ScenarioUser? InstallationOwner = null);
public sealed record ScenarioRole(string Id, string Name)
{
    internal JsonObject Payload() => new()
    {
        ["id"] = Id,
        ["name"] = Name,
        ["color"] = 0,
        ["hoist"] = false,
        ["position"] = 1,
        ["permissions"] = "0",
        ["managed"] = false,
        ["mentionable"] = false,
        ["flags"] = 0,
    };
}

public sealed record ScenarioGuild(string Id, string ChannelId)
{
    internal List<ScenarioUser> Members { get; } = [];
    internal Dictionary<string, string[]> MemberRoles { get; } = [];
}

public sealed partial class ScenarioData
{
    private readonly ScenarioDatabase _database;
    private readonly Func<string, object, Task> _dispatch;
    private readonly DiscordApi _api;
    private long _id = 100000000000000100;

    internal ScenarioData(ScenarioDatabase database, Func<string, object, Task> dispatch, DiscordApi api)
    {
        _database = database;
        _dispatch = dispatch;
        _api = api;
    }

    public async Task<ScenarioUser> UserAsync(long taypoints = 0, string username = "Alice", string? avatar = null, bool botOwner = false)
    {
        ScenarioUser user = new(botOwner ? "100000000000000002" : $"{Interlocked.Increment(ref _id)}", username, avatar);

        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync(
            "INSERT INTO users.users (user_id, username, is_bot, taypoint_count, ignore_until) VALUES (@Id, @Username, false, @taypoints, CURRENT_TIMESTAMP - interval '1 day');",
            new { user.Id, user.Username, taypoints });

        _api.RegisterUser(user);

        return user;
    }

    public ScenarioRole Role(string name = "Regulars") => new($"{Interlocked.Increment(ref _id)}", name);

    public async Task<ScenarioGuild> GuildAsync(ScenarioUser user, long cachedTaypoints = 0, IReadOnlyList<ScenarioRole>? roles = null, string? id = null)
    {
        ScenarioGuild guild = new(id ?? $"{Interlocked.Increment(ref _id)}", $"{Interlocked.Increment(ref _id)}");
        guild.Members.Add(user);
        guild.Members.Add(new(DiscordApi.ApplicationId, "IntegrationBot"));

        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync(
            """
            INSERT INTO guilds.guilds (guild_id, guild_name, prefix) VALUES (@Id, 'Integration guild', '!');
            INSERT INTO guilds.guild_members (guild_id, user_id, last_known_taypoint_count)
            VALUES (@Id, @UserId, @cachedTaypoints);
            """,
            new { guild.Id, UserId = user.Id, cachedTaypoints });

        await _dispatch("GUILD_CREATE", new
        {
            id = guild.Id,
            name = "Integration guild",
            owner_id = user.Id,
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
            roles = new[] { new JsonObject { ["id"] = guild.Id, ["name"] = "@everyone", ["color"] = 0, ["hoist"] = false, ["position"] = 0, ["permissions"] = "8", ["managed"] = false, ["mentionable"] = false, ["flags"] = 0 } }
                .Concat((roles ?? []).Select(role => role.Payload())).ToArray(),
            channels = new[] { new { id = guild.ChannelId, guild_id = guild.Id, type = 0, name = "general", position = 0, permission_overwrites = Array.Empty<object>() } },
            members = guild.Members.Select(member => new { user = DiscordDriver.UserPayload(member), roles = Array.Empty<string>(), joined_at = "2026-01-01T00:00:00+00:00", deaf = false, mute = false, flags = 0 }).ToArray(),
            member_count = 2,
            large = false,
            joined_at = "2026-01-01T00:00:00+00:00",
            presences = Array.Empty<object>(),
            voice_states = Array.Empty<object>(),
            threads = Array.Empty<object>(),
            stage_instances = Array.Empty<object>(),
            guild_scheduled_events = Array.Empty<object>(),
        });

        _api.RegisterGuild(guild, roles ?? []);

        return guild;
    }

    public async Task MemberAsync(ScenarioGuild guild, ScenarioUser user, params ScenarioRole[] roles)
    {
        var existing = guild.Members.Any(member => member.Id == user.Id);
        if (!existing)
        {
            guild.Members.Add(user);
        }

        guild.MemberRoles[user.Id] = [.. roles.Select(role => role.Id)];

        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync("INSERT INTO guilds.guild_members (guild_id, user_id) VALUES (@GuildId, @Id) ON CONFLICT DO NOTHING;",
            new { GuildId = guild.Id, user.Id });

        await _dispatch(existing ? "GUILD_MEMBER_UPDATE" : "GUILD_MEMBER_ADD", new
        {
            guild_id = guild.Id,
            user = DiscordDriver.UserPayload(user),
            roles = guild.MemberRoles[user.Id],
            joined_at = "2026-01-01T00:00:00Z",
            deaf = false,
            mute = false,
        });
    }

    public async Task DisableCommandAsync(string commandName, string reason)
    {
        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync(
            """
            INSERT INTO commands.commands (name, aliases, disabled_message) VALUES (@commandName, ARRAY[]::text[], @reason)
            ON CONFLICT (name) DO UPDATE SET disabled_message = excluded.disabled_message;
            """,
            new { commandName, reason });
    }

    public Task LastFmAsync(ScenarioUser user, string username = "taylorswift") =>
        TextAttributeAsync(user, "lastfm", username);

    public async Task TextAttributeAsync(ScenarioUser user, string attribute, string value)
    {
        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync("""
            INSERT INTO attributes.text_attributes (user_id, attribute_id, attribute_value) VALUES (@Id, @attribute, @value)
            ON CONFLICT (user_id, attribute_id) DO UPDATE SET attribute_value = excluded.attribute_value;
            """, new { user.Id, attribute, value });
    }

    public async Task UsernameHistoryAsync(ScenarioUser user, string oldName = "Enchanted13", bool hidden = false)
    {
        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync("""
            INSERT INTO users.usernames (user_id, username, changed_at) VALUES (@Id, @oldName, CURRENT_TIMESTAMP - interval '1 day');
            INSERT INTO users.username_history_configuration (user_id, is_hidden) VALUES (@Id, @hidden);
            """, new { user.Id, oldName, hidden });
    }

    public async Task WillAsync(ScenarioUser owner, ScenarioUser beneficiary, int inactiveDays = 21)
    {
        var guild = await GuildAsync(owner);
        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync("""
            INSERT INTO users.taypoint_wills (owner_user_id, beneficiary_user_id) VALUES (@OwnerId, @BeneficiaryId);
            UPDATE guilds.guild_members SET last_spoke_at = CURRENT_TIMESTAMP - @inactiveDays * interval '1 day'
            WHERE guild_id = @GuildId AND user_id = @OwnerId;
            """, new { OwnerId = owner.Id, BeneficiaryId = beneficiary.Id, GuildId = guild.Id, inactiveDays });
    }

    public async Task ActiveTodayAsync(ScenarioUser user)
    {
        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync("UPDATE guilds.guild_members SET last_spoke_at = CURRENT_TIMESTAMP WHERE user_id = @Id;", new { user.Id });
    }

    public async Task DailyStreakAsync(ScenarioUser user, int previousStreak)
    {
        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync("""
            INSERT INTO users.daily_payouts (user_id, last_payout_at, streak_count, max_streak_count)
            VALUES (@Id, date_trunc('day', CURRENT_TIMESTAMP) - interval '12 hours', @previousStreak, @previousStreak);
            """, new { user.Id, previousStreak });
    }

    public async Task PlusAsync(ScenarioUser user, int maximumGuilds = 2, params ScenarioGuild[] activeGuilds)
    {
        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync("""
            INSERT INTO plus.plus_users (user_id, active, max_plus_guilds, source) VALUES (@Id, true, @maximumGuilds, 'manual_dont_reward');
            """, new { user.Id, maximumGuilds });

        foreach (var guild in activeGuilds)
        {
            await connection.ExecuteAsync("INSERT INTO plus.plus_guilds (guild_id, plus_user_id, state) VALUES (@GuildId, @Id, 'enabled');",
                new { GuildId = guild.Id, user.Id });
        }
    }

    public async Task AccessibleRoleAsync(ScenarioGuild guild, ScenarioRole role, string? group = null)
    {
        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync("""
            INSERT INTO guilds.guild_accessible_roles (guild_id, role_id, accessible, group_name) VALUES (@GuildId, @RoleId, true, @group);
            """, new { GuildId = guild.Id, RoleId = role.Id, group });
    }

    public async Task JailRoleAsync(ScenarioGuild guild, ScenarioRole role)
    {
        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync("INSERT INTO guilds.jail_roles (guild_id, jail_role_id, set_at) VALUES (@GuildId, @RoleId, CURRENT_TIMESTAMP);",
            new { GuildId = guild.Id, RoleId = role.Id });
    }

    public async Task ModerationLogAsync(ScenarioGuild guild)
    {
        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync("INSERT INTO moderation.mod_log_channels (guild_id, channel_id) VALUES (@Id, @ChannelId);", guild);

        _api.ExpectModerationLog(guild);
    }

    public async Task KnownCommandAsync(string name)
    {
        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync("INSERT INTO commands.commands (name, aliases, disabled_message) VALUES (@name, ARRAY[]::text[], '') ON CONFLICT DO NOTHING;", new { name });
    }

    public async Task LocationAsync(ScenarioUser user)
    {
        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync("""
            INSERT INTO attributes.location_attributes (user_id, formatted_address, longitude, latitude, timezone_id)
            VALUES (@Id, 'Quebec City, QC, Canada', '-71.2074596', '46.8130816', 'America/Toronto');
            """, new { user.Id });
    }

    public async Task PopulationAsync(ScenarioGuild guild)
    {
        int[] ages = [10, 11, 12, 13, 15, 15, 20, 24, 40, 60];
        for (var index = 0; index < ages.Length; index++)
        {
            var user = await UserAsync(username: $"Member{index}");
            await MemberAsync(guild, user);
            await TextAttributeAsync(user, "gender", index < 5 ? "Male" : index < 8 ? "Female" : "Other");

            await using var connection = _database.CreateConnection();
            await connection.ExecuteAsync("INSERT INTO attributes.birthdays (user_id, birthday) VALUES (@Id, (CURRENT_DATE - @Age * interval '1 year')::date);",
                new { user.Id, Age = ages[index] });
        }
    }
}
