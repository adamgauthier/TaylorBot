using Dapper;
using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.EntityTracker.Infrastructure.TextChannel
{
    public class TextChannelRepository : ITextChannelRepository
    {
        private readonly PostgresConnectionFactory _postgresConnectionFactory;

        public TextChannelRepository(PostgresConnectionFactory postgresConnectionFactory)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
        }

        public async ValueTask AddTextChannelAsync(ITextChannel textChannel)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                "INSERT INTO guilds.text_channels (guild_id, channel_id) VALUES (@GuildId, @ChannelId);",
                new
                {
                    GuildId = textChannel.GuildId.ToString(),
                    ChannelId = textChannel.Id.ToString()
                }
            );
        }

        public async Task AddTextChannelIfNotAddedAsync(ITextChannel textChannel)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                "INSERT INTO guilds.text_channels (guild_id, channel_id) VALUES (@GuildId, @ChannelId) ON CONFLICT (guild_id, channel_id) DO NOTHING;",
                new
                {
                    GuildId = textChannel.GuildId.ToString(),
                    ChannelId = textChannel.Id.ToString()
                }
            );
        }
    }
}
