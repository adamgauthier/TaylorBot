using Dapper;
using Discord;
using Discord.Commands;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Infrastructure
{
    public class DisabledGuildChannelCommandPostgresRepository : IDisabledGuildChannelCommandRepository
    {
        private readonly PostgresConnectionFactory _postgresConnectionFactory;

        public DisabledGuildChannelCommandPostgresRepository(PostgresConnectionFactory postgresConnectionFactory)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
        }

        public async ValueTask<bool> IsGuildChannelCommandDisabledAsync(ITextChannel textChannel, CommandInfo command)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            var disabled = await connection.QuerySingleOrDefaultAsync<bool>(
                @"SELECT EXISTS(
                    SELECT 1 FROM guilds.channel_commands
                    WHERE guild_id = @GuildId AND channel_id = @ChannelId AND command_id = @CommandId
                );",
                new
                {
                    GuildId = textChannel.GuildId.ToString(),
                    ChannelId = textChannel.Id.ToString(),
                    CommandId = command.Aliases.First()
                }
            );

            return disabled;
        }
    }
}
