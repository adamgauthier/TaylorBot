using Dapper;
using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.EntityTracker.Infrastructure.TextChannel
{
    public class SpamChannelPostgresRepository : ISpamChannelRepository
    {
        private readonly PostgresConnectionFactory _postgresConnectionFactory;

        public SpamChannelPostgresRepository(PostgresConnectionFactory postgresConnectionFactory)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
        }

        public async ValueTask<bool> InsertOrGetIsSpamChannelAsync(ITextChannel channel)
        {
            await using var connection = _postgresConnectionFactory.CreateConnection();

            return await connection.QuerySingleAsync<bool>(
                @"INSERT INTO guilds.text_channels (guild_id, channel_id) VALUES (@GuildId, @ChannelId)
                ON CONFLICT (guild_id, channel_id) DO UPDATE SET
                    registered_at = guilds.text_channels.registered_at
                RETURNING is_spam;",
                new
                {
                    GuildId = channel.GuildId.ToString(),
                    ChannelId = channel.Id.ToString()
                }
            );
        }
    }
}
