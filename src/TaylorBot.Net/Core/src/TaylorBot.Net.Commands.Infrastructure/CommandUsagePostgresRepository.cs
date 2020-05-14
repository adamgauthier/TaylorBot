using Dapper;
using Discord.Commands;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Infrastructure.Options;
using TaylorBot.Net.Core.Tasks;

namespace TaylorBot.Net.Commands.Infrastructure
{
    public class CommandUsagePostgresRepository : PostgresRepository, ICommandUsageRepository
    {
        private readonly ConcurrentDictionary<string, CommandUsage> _usageCache = new ConcurrentDictionary<string, CommandUsage>();
        private readonly TaskExceptionLogger _taskExceptionLogger;

        public CommandUsagePostgresRepository(IOptionsMonitor<DatabaseConnectionOptions> optionsMonitor, TaskExceptionLogger taskExceptionLogger) : base(optionsMonitor)
        {
            _taskExceptionLogger = taskExceptionLogger;
            Task.Run(async () =>
                await _taskExceptionLogger.LogOnError(async () =>
                {
                    while (true)
                    {
                        foreach (var usageKey in _usageCache.Keys)
                        {
                            if (_usageCache.TryRemove(usageKey, out var removedUsage))
                            {
                                await UpdateUsageCountAsync(usageKey, removedUsage);
                            }
                        }
                        await Task.Delay(TimeSpan.FromMinutes(5));
                    }
                }, nameof(CommandUsagePostgresRepository))
            );
        }

        public void AddSuccessfulUseCount(CommandInfo command)
        {
            _usageCache.AddOrUpdate(
                command.Aliases.First(),
                new CommandUsage(successfulUseCount: 1, unhandledErrorCount: 0),
                (name, usage) => usage.IncrementUseCount()
            );
        }

        public void AddUnhandledErrorCount(CommandInfo command)
        {
            _usageCache.AddOrUpdate(
                command.Aliases.First(),
                new CommandUsage(successfulUseCount: 0, unhandledErrorCount: 1),
                (name, usage) => usage.IncrementErrorCount()
            );
        }

        private async ValueTask UpdateUsageCountAsync(string commandName, CommandUsage commandUsage)
        {
            using var connection = Connection;

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

        private class CommandUsage
        {
            public long SuccessfulUseCount { get; }
            public long UnhandledErrorCount { get; }

            public CommandUsage(long successfulUseCount, long unhandledErrorCount)
            {
                SuccessfulUseCount = successfulUseCount;
                UnhandledErrorCount = unhandledErrorCount;
            }

            public CommandUsage IncrementUseCount() => new CommandUsage(SuccessfulUseCount + 1, UnhandledErrorCount);
            public CommandUsage IncrementErrorCount() => new CommandUsage(SuccessfulUseCount, UnhandledErrorCount + 1);
        }
    }
}
