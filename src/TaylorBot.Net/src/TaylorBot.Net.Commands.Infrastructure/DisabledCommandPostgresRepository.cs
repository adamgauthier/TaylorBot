using Dapper;
using Discord.Commands;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Infrastructure.Options;

namespace TaylorBot.Net.Commands.Infrastructure
{
    public class DisabledCommandPostgresRepository : PostgresRepository, IDisabledCommandRepository
    {
        public DisabledCommandPostgresRepository(IOptionsMonitor<DatabaseConnectionOptions> optionsMonitor) : base(optionsMonitor)
        {
        }

        public async Task<bool> InsertOrGetIsCommandDisabledAsync(CommandInfo command)
        {
            using var connection = Connection;
            connection.Open();

            var enabled = await connection.QuerySingleOrDefaultAsync<bool>(
                @"INSERT INTO commands.commands (name) VALUES (@CommandName)
                ON CONFLICT (name) DO UPDATE SET enabled = commands.commands.enabled
                RETURNING enabled;",
                new
                {
                    CommandName = command.Name
                }
            );

            return !enabled;
        }
    }
}
