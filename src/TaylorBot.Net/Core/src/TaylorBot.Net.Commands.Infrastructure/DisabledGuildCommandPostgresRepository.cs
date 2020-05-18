using Dapper;
using Discord;
using Discord.Commands;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Infrastructure
{
    public class DisabledGuildCommandPostgresRepository : IDisabledGuildCommandRepository
    {
        private readonly PostgresConnectionFactory _postgresConnectionFactory;

        public DisabledGuildCommandPostgresRepository(PostgresConnectionFactory postgresConnectionFactory)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
        }

        public async Task<bool> IsGuildCommandDisabledAsync(IGuild guild, CommandInfo command)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            var disabled = await connection.QuerySingleOrDefaultAsync<bool>(
                @"SELECT EXISTS(
                    SELECT 1 FROM guilds.guild_commands
                    WHERE guild_id = @GuildId AND command_name = @CommandName AND disabled = TRUE
                );",
                new
                {
                    GuildId = guild.Id.ToString(),
                    CommandName = command.Aliases.First()
                }
            );

            return disabled;
        }
    }
}
