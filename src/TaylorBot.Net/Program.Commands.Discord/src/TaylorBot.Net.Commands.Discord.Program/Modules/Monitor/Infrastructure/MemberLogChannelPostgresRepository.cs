using Dapper;
using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Logs.Domain;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Logs.Infrastructure
{
    public class MemberLogChannelPostgresRepository : IMemberLogChannelRepository
    {
        private readonly PostgresConnectionFactory _postgresConnectionFactory;

        public MemberLogChannelPostgresRepository(PostgresConnectionFactory postgresConnectionFactory)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
        }

        public async ValueTask AddOrUpdateMemberLogAsync(ITextChannel textChannel)
        {
            await using var connection = _postgresConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                @"INSERT INTO plus.member_log_channels (guild_id, member_log_channel_id)
                VALUES (@GuildId, @ChannelId)
                ON CONFLICT (guild_id) DO UPDATE SET
                    member_log_channel_id = excluded.member_log_channel_id;",
                new
                {
                    GuildId = textChannel.GuildId.ToString(),
                    ChannelId = textChannel.Id.ToString()
                }
            );
        }

        private class LogChannelDto
        {
            public string member_log_channel_id { get; set; } = null!;
        }

        public async ValueTask<MemberLog?> GetMemberLogForGuildAsync(IGuild guild)
        {
            await using var connection = _postgresConnectionFactory.CreateConnection();

            var logChannel = await connection.QuerySingleOrDefaultAsync<LogChannelDto?>(
                @"SELECT member_log_channel_id FROM plus.member_log_channels
                WHERE guild_id = @GuildId;",
                new
                {
                    GuildId = guild.Id.ToString()
                }
            );

            return logChannel != null ? new MemberLog(new SnowflakeId(logChannel.member_log_channel_id)) : null;
        }

        public async ValueTask RemoveMemberLogAsync(IGuild guild)
        {
            await using var connection = _postgresConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                "DELETE FROM plus.member_log_channels WHERE guild_id = @GuildId;",
                new
                {
                    GuildId = guild.Id.ToString()
                }
            );
        }
    }
}
