using Dapper;
using System.Collections.Concurrent;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Infrastructure
{
    public class CommandUsagePostgresRepository : ICommandUsageRepository
    {
        private readonly PostgresConnectionFactory _postgresConnectionFactory;
        private readonly ConcurrentDictionary<string, CommandUsage> _usageCache = new ConcurrentDictionary<string, CommandUsage>();

        public CommandUsagePostgresRepository(PostgresConnectionFactory postgresConnectionFactory)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
        }

        public void QueueIncrementSuccessfulUseCount(string commandName)
        {
            _usageCache.AddOrUpdate(
                commandName,
                new CommandUsage(SuccessfulUseCount: 1, UnhandledErrorCount: 0),
                (name, usage) => usage with { SuccessfulUseCount = usage.SuccessfulUseCount + 1 }
            );
        }

        public void QueueIncrementUnhandledErrorCount(string commandName)
        {
            _usageCache.AddOrUpdate(
                commandName,
                new CommandUsage(SuccessfulUseCount: 0, UnhandledErrorCount: 1),
                (name, usage) => usage with { SuccessfulUseCount = usage.UnhandledErrorCount + 1 }
            );
        }

        public async ValueTask PersistQueuedUsageCountIncrementsAsync()
        {
            foreach (var usageKey in _usageCache.Keys)
            {
                if (_usageCache.TryRemove(usageKey, out var removedUsage))
                {
                    await UpdateUsageCountAsync(usageKey, removedUsage);
                }
            }
        }

        private async ValueTask UpdateUsageCountAsync(string commandName, CommandUsage commandUsage)
        {
            await using var connection = _postgresConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                @"UPDATE commands.commands SET
                    successful_use_count = successful_use_count + @UseCountToAdd,
                    unhandled_error_count = unhandled_error_count + @ErrorCountToAdd
                WHERE name = @CommandName;",
                new
                {
                    CommandName = commandName,
                    UseCountToAdd = commandUsage.SuccessfulUseCount,
                    ErrorCountToAdd = commandUsage.UnhandledErrorCount
                }
            );
        }

        private record CommandUsage(long SuccessfulUseCount, long UnhandledErrorCount);
    }
}
