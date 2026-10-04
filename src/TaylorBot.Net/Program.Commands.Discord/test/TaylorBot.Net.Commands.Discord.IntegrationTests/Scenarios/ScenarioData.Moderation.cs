using Dapper;
using System.Text.Json;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Scenarios;

public sealed partial class ScenarioData
{
    public async Task LogChannelAsync(ScenarioGuild guild, string kind, string? channelId = null)
    {
        var (table, column) = ModerationTables.Log(kind);
        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync(
            $"INSERT INTO {table} (guild_id, {column}) VALUES (@Id, @ChannelId) ON CONFLICT (guild_id) DO UPDATE SET {column} = excluded.{column};",
            new { guild.Id, ChannelId = channelId ?? guild.ChannelId });
    }

    public async Task BlockedModmailAsync(ScenarioGuild guild, ScenarioUser user)
    {
        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync(
            "INSERT INTO moderation.mod_mail_blocked_users (guild_id, user_id) VALUES (@GuildId, @Id) ON CONFLICT DO NOTHING;",
            new { GuildId = guild.Id, user.Id });
    }

    public async Task ModmailBlockLimitAsync(ScenarioGuild guild)
    {
        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync(
            """
            INSERT INTO moderation.mod_mail_blocked_users (guild_id, user_id)
            SELECT @Id, (200000000000000000 + n)::text FROM generate_series(1, 50) n;
            """, new { guild.Id });
    }

    public async Task DisabledServerCommandAsync(ScenarioGuild guild, string command)
    {
        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync(
            "INSERT INTO guilds.guild_commands (guild_id, command_name, disabled) VALUES (@Id, @command, true) ON CONFLICT (guild_id, command_name) DO UPDATE SET disabled = true;",
            new { guild.Id, command });
    }

    public async Task FeedbackEligibleAsync(ScenarioGuild guild, ScenarioUser user)
    {
        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync(
            """
            UPDATE guilds.guild_members SET alive = true, message_count = 1301, minute_count = 1301,
            first_joined_at = CURRENT_TIMESTAMP - interval '31 days', last_spoke_at = CURRENT_TIMESTAMP
            WHERE guild_id = @GuildId AND user_id = @Id;
            """, new { GuildId = guild.Id, user.Id });
    }

    public async Task YearbookAsync(ScenarioUser user, bool isMod = false, bool completed = false)
    {
        await using var connection = _database.CreateConnection();
        var value = JsonSerializer.Serialize(new { members = new[] { new { userId = user.Id, isMod, processedInfo = new { completed } } } });
        await connection.ExecuteAsync(
            "INSERT INTO configuration.application_info (info_key, info_value) VALUES ('rewardyearbook2025', @value) ON CONFLICT (info_key) DO UPDATE SET info_value = excluded.info_value;",
            new { value });
    }
}

internal static class ModerationTables
{
    internal static (string Table, string Column) Log(string kind) => kind switch
    {
        "mod" => ("moderation.mod_log_channels", "channel_id"),
        "modmail" => ("moderation.mod_mail_log_channels", "channel_id"),
        "members" => ("plus.member_log_channels", "member_log_channel_id"),
        "deleted" => ("plus.deleted_log_channels", "deleted_log_channel_id"),
        "edited" => ("plus.edited_log_channels", "edited_log_channel_id"),
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };
}
