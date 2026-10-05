using Dapper;
using TaylorBot.Net.UserNotifier.Program.Jobs;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.Scenarios;

public sealed partial class ScenarioData
{
    public async Task<IAsyncDisposable> RejectTrackingWritesAsync(UserNotifierJob job, ScenarioUser? onlyUser = null)
    {
        var (table, condition) = job switch
        {
            UserNotifierJob.LastSpoke => ("guilds.guild_members", "last_spoke_at IS NULL"),
            UserNotifierJob.ChannelMessages => ("guilds.text_channels", "message_count = 0"),
            UserNotifierJob.MemberMessages => ("guilds.guild_members", "message_count = 0"),
            UserNotifierJob.Minutes => ("guilds.guild_members", "minute_count = 0"),
            _ => throw new ArgumentOutOfRangeException(nameof(job)),
        };

        if (onlyUser != null)
        {
            if (job == UserNotifierJob.ChannelMessages)
            {
                throw new ArgumentException("Channel counters cannot be restricted to a user.", nameof(onlyUser));
            }

            condition += $" OR user_id <> '{ulong.Parse(onlyUser.Id)}'";
        }

        await using var connection = database.CreateConnection();
        await connection.ExecuteAsync($"ALTER TABLE {table} ADD CONSTRAINT integration_reject_tracking CHECK ({condition});");

        return new ReversibleDatabaseChange(database, $"ALTER TABLE {table} DROP CONSTRAINT integration_reject_tracking;");
    }

    public async Task<IAsyncDisposable> UnavailableBirthdayCalendarAsync()
    {
        await using var connection = database.CreateConnection();
        await connection.ExecuteAsync("ALTER MATERIALIZED VIEW attributes.birthday_calendar_6months RENAME TO integration_unavailable_calendar;");

        return new ReversibleDatabaseChange(database, "ALTER MATERIALIZED VIEW attributes.integration_unavailable_calendar RENAME TO birthday_calendar_6months;");
    }

    public async Task<IAsyncDisposable> UnavailableModLogConfigurationAsync()
    {
        await using var connection = database.CreateConnection();
        await connection.ExecuteAsync("ALTER TABLE moderation.mod_log_channels RENAME TO integration_unavailable_mod_log;");

        return new ReversibleDatabaseChange(database, "ALTER TABLE moderation.integration_unavailable_mod_log RENAME TO mod_log_channels;");
    }

    private sealed class ReversibleDatabaseChange(ScenarioDatabase database, string undoSql) : IAsyncDisposable
    {
        private bool _disposed;

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                await using var connection = database.CreateConnection();
                await connection.ExecuteAsync(undoSql);
                _disposed = true;
            }
        }
    }
}
