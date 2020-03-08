using Dapper;
using Discord;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Infrastructure.Options;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.MemberLogging.Domain.TextChannel;

namespace TaylorBot.Net.MemberLogging.Infrastructure
{
    public class MemberLoggingChannelRepository : PostgresRepository, IMemberLoggingChannelRepository
    {
        public MemberLoggingChannelRepository(IOptionsMonitor<DatabaseConnectionOptions> optionsMonitor) : base(optionsMonitor)
        {
        }

        public async Task<IEnumerable<LogChannel>> GetLogChannelsForGuildAsync(IGuild guild)
        {
            using var connection = Connection;

            var logChannels = await connection.QueryAsync<LogChannelDto>(
                "SELECT channel_id FROM guilds.text_channels WHERE guild_id = @GuildId AND is_member_log = TRUE;",
                new
                {
                    GuildId = guild.Id.ToString()
                }
            );

            return logChannels.Select(logChannel => new LogChannel(new SnowflakeId(logChannel.channel_id)));
        }
    }
}
