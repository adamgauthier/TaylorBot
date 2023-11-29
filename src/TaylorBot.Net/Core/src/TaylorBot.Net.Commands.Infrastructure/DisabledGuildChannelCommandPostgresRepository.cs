using Dapper;
using Discord;
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

        public async ValueTask DisableInAsync(MessageChannel channel, IGuild guild, string commandName)
        {
            await using var connection = _postgresConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                @"INSERT INTO guilds.channel_commands(guild_id, channel_id, command_id)
                VALUES(@GuildId, @ChannelId, @CommandId) ON CONFLICT DO NOTHING;",
                new
                {
                    GuildId = guild.Id.ToString(),
                    ChannelId = channel.Id.ToString(),
                    CommandId = commandName
                }
            );
        }

        public async ValueTask EnableInAsync(MessageChannel channel, IGuild guild, string commandName)
        {
            await using var connection = _postgresConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                @"DELETE FROM guilds.channel_commands
                WHERE guild_id = @GuildId AND channel_id = @ChannelId AND command_id = @CommandId;",
                new
                {
                    GuildId = guild.Id.ToString(),
                    ChannelId = channel.Id.ToString(),
                    CommandId = commandName
                }
            );
        }

        public async ValueTask<bool> IsGuildChannelCommandDisabledAsync(MessageChannel channel, IGuild guild, CommandMetadata command)
        {
            await using var connection = _postgresConnectionFactory.CreateConnection();

            var disabled = await connection.QuerySingleOrDefaultAsync<bool>(
                """
                SELECT EXISTS(
                    SELECT 1 FROM guilds.channel_commands
                    WHERE guild_id = @GuildId AND channel_id = @ChannelId AND command_id = @CommandId
                );
                """,
                new
                {
                    GuildId = guild.Id.ToString(),
                    ChannelId = channel.Id.ToString(),
                    CommandId = command.Name
                }
            );

            return disabled;
        }
    }
}
