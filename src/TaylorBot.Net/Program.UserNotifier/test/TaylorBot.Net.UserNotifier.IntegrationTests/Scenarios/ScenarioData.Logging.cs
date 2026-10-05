using Dapper;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.Scenarios;

public sealed partial class ScenarioData
{
    public async Task ModLogAsync(ScenarioGuild guild, string? channelId = null)
    {
        await using var connection = database.CreateConnection();
        await connection.ExecuteAsync(
            """
            INSERT INTO moderation.mod_log_channels (guild_id, channel_id) VALUES (@Id, @ChannelId)
            ON CONFLICT (guild_id) DO UPDATE SET channel_id = excluded.channel_id;
            """, new { guild.Id, ChannelId = channelId ?? guild.ChannelId });
    }

    public async Task ExpireLogChannelCacheAsync(ScenarioGuild guild)
    {
        await database.Redis.KeyDeleteAsync([$"deleted-logs:guild:{guild.Id}", $"edited-logs:guild:{guild.Id}"]);
    }

    public async Task LogChannelAsync(ScenarioGuild guild, string kind, bool enabled = true)
    {
        var (table, column) = kind switch
        {
            "members" => ("plus.member_log_channels", "member_log_channel_id"),
            "deleted" => ("plus.deleted_log_channels", "deleted_log_channel_id"),
            "edited" => ("plus.edited_log_channels", "edited_log_channel_id"),
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };

        await using var connection = database.CreateConnection();
        await connection.ExecuteAsync($"INSERT INTO {table} (guild_id, {column}) VALUES (@Id, @ChannelId);", new { guild.Id, guild.ChannelId });

        if (enabled)
        {
            await connection.ExecuteAsync(
                """
                INSERT INTO plus.plus_users (user_id, active, max_plus_guilds, source)
                VALUES (@UserId, true, 2, 'manual_dont_reward') ON CONFLICT DO NOTHING;
                INSERT INTO plus.plus_guilds (guild_id, plus_user_id, state)
                VALUES (@Id, @UserId, 'enabled') ON CONFLICT DO NOTHING;
                """, new { guild.Id, UserId = guild.Owner.Id });
        }
    }
}
