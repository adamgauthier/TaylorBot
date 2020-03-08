using Dapper;
using Discord;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Infrastructure.Options;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.MessageLogging.Domain.TextChannel;

namespace TaylorBot.Net.MessageLogging.Infrastructure
{
    public class MessageLoggingChannelRepository : PostgresRepository, IMessageLoggingChannelRepository
    {
        public MessageLoggingChannelRepository(IOptionsMonitor<DatabaseConnectionOptions> optionsMonitor) : base(optionsMonitor)
        {
        }

        private class LogChannelDto
        {
            public string channel_id { get; set; }
        }

        public async Task<IEnumerable<LogChannel>> GetMessageLogChannelsForGuildAsync(IGuild guild)
        {
            using var connection = Connection;

            var logChannels = await connection.QueryAsync<LogChannelDto>(
                "SELECT channel_id FROM guilds.text_channels WHERE guild_id = @GuildId AND is_message_log = TRUE;",
                new
                {
                    GuildId = guild.Id.ToString()
                }
            );

            return logChannels.Select(logChannel => new LogChannel(new SnowflakeId(logChannel.channel_id)));
        }
    }
}
