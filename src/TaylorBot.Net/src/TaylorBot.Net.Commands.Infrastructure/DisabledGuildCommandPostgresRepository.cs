using Dapper;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Infrastructure.Options;

namespace TaylorBot.Net.Commands.Infrastructure
{
    public class DisabledGuildCommandPostgresRepository : PostgresRepository, IDisabledGuildCommandRepository
    {
        public DisabledGuildCommandPostgresRepository(IOptionsMonitor<DatabaseConnectionOptions> optionsMonitor) : base(optionsMonitor)
        {
        }

        public async Task<bool> IsGuildCommandDisabledAsync(IGuild guild, CommandInfo command)
        {
            using var connection = Connection;
            connection.Open();

            var disabled = await connection.QuerySingleOrDefaultAsync<bool>(
                @"SELECT EXISTS(
                    SELECT 1 FROM guilds.guild_commands
                    WHERE guild_id = @GuildId AND command_name = @CommandName AND disabled = TRUE
                );",
                new
                {
                    GuildId = guild.Id.ToString(),
                    CommandName = command.Name
                }
            );

            return disabled;
        }
    }
}
