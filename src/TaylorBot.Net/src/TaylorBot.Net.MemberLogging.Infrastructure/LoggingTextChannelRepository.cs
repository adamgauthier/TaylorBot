using Discord;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Infrastructure.Options;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;
using Dapper;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.MemberLogging.Infrastructure
{
    public class LoggingTextChannelRepository : PostgresRepository, ILoggingTextChannelRepository
    {
        public LoggingTextChannelRepository(IOptionsMonitor<DatabaseConnectionOptions> optionsMonitor) : base(optionsMonitor)
        {
        }

        public async Task<IEnumerable<LogChannel>> GetLogChannelsForGuildAsync(IGuild guild)
        {
            using (var connection = Connection)
            {
                connection.Open();

                var logChannels = await connection.QueryAsync<LogChannelDto>(
                    "SELECT channel_id FROM guilds.text_channels WHERE guild_id = @GuildId AND is_log = TRUE;",
                    new
                    {
                        GuildId = guild.Id.ToString()
                    }
                );

                return logChannels.Select(logChannel => new LogChannel(new SnowflakeId(logChannel.channel_id)));
            }
        }
    }
}
