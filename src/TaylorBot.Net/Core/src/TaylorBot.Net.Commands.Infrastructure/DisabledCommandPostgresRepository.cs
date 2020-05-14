using Dapper;
using Discord.Commands;
using Microsoft.Extensions.Options;
using System.Linq;
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

            var enabled = await connection.QuerySingleOrDefaultAsync<bool>(
                @"INSERT INTO commands.commands (name, aliases, module_name) VALUES (@CommandName, @Aliases, @ModuleName)
                ON CONFLICT (name) DO UPDATE SET
                    aliases = excluded.aliases,
                    module_name = excluded.module_name
                RETURNING enabled;",
                new
                {
                    CommandName = command.Aliases.First(),
                    Aliases = command.Aliases,
                    ModuleName = command.Module.Name
                }
            );

            return !enabled;
        }
    }
}
