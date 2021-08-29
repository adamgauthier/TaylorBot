using Dapper;
using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Monitor.Domain;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Monitor.Infrastructure
{
    public class DeletedLogChannelPostgresRepository : IDeletedLogChannelRepository
    {
        private readonly PostgresConnectionFactory _postgresConnectionFactory;

        public DeletedLogChannelPostgresRepository(PostgresConnectionFactory postgresConnectionFactory)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
        }

        public async ValueTask AddOrUpdateDeletedLogAsync(ITextChannel textChannel)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                @"INSERT INTO plus.deleted_log_channels (guild_id, deleted_log_channel_id)
                VALUES (@GuildId, @ChannelId)
                ON CONFLICT (guild_id) DO UPDATE SET
                    deleted_log_channel_id = excluded.deleted_log_channel_id;",
                new
                {
                    GuildId = textChannel.GuildId.ToString(),
                    ChannelId = textChannel.Id.ToString()
                }
            );
        }

        private class LogChannelDto
        {
            public string deleted_log_channel_id { get; set; } = null!;
        }

        public async ValueTask<DeletedLog?> GetDeletedLogForGuildAsync(IGuild guild)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            var logChannel = await connection.QuerySingleOrDefaultAsync<LogChannelDto?>(
                @"SELECT deleted_log_channel_id FROM plus.deleted_log_channels
                WHERE guild_id = @GuildId;",
                new
                {
                    GuildId = guild.Id.ToString()
                }
            );

            return logChannel != null ? new DeletedLog(new SnowflakeId(logChannel.deleted_log_channel_id)) : null;
        }

        public async ValueTask RemoveDeletedLogAsync(IGuild guild)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                "DELETE FROM plus.deleted_log_channels WHERE guild_id = @GuildId;",
                new
                {
                    GuildId = guild.Id.ToString()
                }
            );
        }
    }
}
