using Dapper;
using Discord;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.MemberLogging.Domain.TextChannel;

namespace TaylorBot.Net.MemberLogging.Infrastructure
{
    public class MemberLoggingChannelRepository : IMemberLoggingChannelRepository
    {
        private readonly PostgresConnectionFactory _postgresConnectionFactory;

        public MemberLoggingChannelRepository(PostgresConnectionFactory postgresConnectionFactory)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
        }

        public async ValueTask<IReadOnlyCollection<LogChannel>> GetLogChannelsForGuildAsync(IGuild guild)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            var logChannels = await connection.QueryAsync<LogChannelDto>(
                "SELECT channel_id FROM guilds.text_channels WHERE guild_id = @GuildId AND is_member_log = TRUE;",
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
