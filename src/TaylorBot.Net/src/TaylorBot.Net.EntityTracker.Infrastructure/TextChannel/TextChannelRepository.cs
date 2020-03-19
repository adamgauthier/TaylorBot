using Dapper;
using Discord;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Infrastructure.Options;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.EntityTracker.Infrastructure.TextChannel
{
    public class TextChannelRepository : PostgresRepository, ITextChannelRepository
    {
        public TextChannelRepository(IOptionsMonitor<DatabaseConnectionOptions> optionsMonitor) : base(optionsMonitor)
        {
        }

        public async ValueTask AddTextChannelAsync(ITextChannel textChannel)
        {
            using var connection = Connection;

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
            using var connection = Connection;

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
