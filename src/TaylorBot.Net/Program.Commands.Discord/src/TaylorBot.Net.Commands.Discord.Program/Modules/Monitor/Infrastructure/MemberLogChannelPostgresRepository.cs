using Dapper;
using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Monitor.Domain;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Monitor.Infrastructure;

public class MemberLogChannelPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : IMemberLogChannelRepository
{
    public async ValueTask AddOrUpdateMemberLogAsync(GuildTextChannel textChannel)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            """
            INSERT INTO plus.member_log_channels (guild_id, member_log_channel_id)
            VALUES (@GuildId, @ChannelId)
            ON CONFLICT (guild_id) DO UPDATE SET
                member_log_channel_id = excluded.member_log_channel_id;
            """,
            new
            {
                GuildId = $"{textChannel.GuildId}",
                ChannelId = $"{textChannel.Id}",
            }
        );
    }

    private sealed class LogChannelDto
    {
        public string member_log_channel_id { get; set; } = null!;
    }

    public async ValueTask<MemberLog?> GetMemberLogForGuildAsync(IGuild guild)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var logChannel = await connection.QuerySingleOrDefaultAsync<LogChannelDto?>(
            """
            SELECT member_log_channel_id FROM plus.member_log_channels
            WHERE guild_id = @GuildId;
            """,
            new
            {
                GuildId = $"{guild.Id}",
            }
        );

        return logChannel != null ? new MemberLog(new SnowflakeId(logChannel.member_log_channel_id)) : null;
    }

    public async ValueTask RemoveMemberLogAsync(IGuild guild)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            "DELETE FROM plus.member_log_channels WHERE guild_id = @GuildId;",
            new
            {
                GuildId = $"{guild.Id}",
            }
        );
    }
}
