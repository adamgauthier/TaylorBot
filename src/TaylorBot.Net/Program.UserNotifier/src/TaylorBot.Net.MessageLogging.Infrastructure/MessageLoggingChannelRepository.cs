using Dapper;
using Discord;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.MessageLogging.Domain.TextChannel;

namespace TaylorBot.Net.MessageLogging.Infrastructure
{
    public class MessageLoggingChannelRepository : IMessageLoggingChannelRepository
    {
        private readonly PostgresConnectionFactory _postgresConnectionFactory;

        public MessageLoggingChannelRepository(PostgresConnectionFactory postgresConnectionFactory)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
        }

        private class LogChannelDto
        {
            public string channel_id { get; set; } = null!;
        }

        public async ValueTask<IReadOnlyCollection<LogChannel>> GetMessageLogChannelsForGuildAsync(IGuild guild)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            var logChannels = await connection.QueryAsync<LogChannelDto>(
                "SELECT channel_id FROM guilds.text_channels WHERE guild_id = @GuildId AND is_message_log = TRUE;",
                new
                {
                    GuildId = guild.Id.ToString()
                }
            );

            return logChannels.Select(
                logChannel => new LogChannel(new SnowflakeId(logChannel.channel_id))
            ).ToList();
        }
    }
}
