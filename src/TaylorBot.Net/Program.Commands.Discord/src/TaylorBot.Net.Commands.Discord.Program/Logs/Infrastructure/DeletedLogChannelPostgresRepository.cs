using Dapper;
using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Logs.Domain;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Discord.Program.Logs.Infrastructure
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
