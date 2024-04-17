using Dapper;
using Discord;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.EntityTracker.Domain.Member;

namespace TaylorBot.Net.EntityTracker.Infrastructure.Member;

public class MemberRepository(PostgresConnectionFactory postgresConnectionFactory) : IMemberRepository
{
    public async ValueTask<bool> AddNewMemberAsync(IGuildUser member)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var result = await connection.QuerySingleOrDefaultAsync<bool>(
            """
            INSERT INTO guilds.guild_members (guild_id, user_id, first_joined_at) VALUES (@GuildId, @UserId, @FirstJoinedAt)
            ON CONFLICT (guild_id, user_id) DO NOTHING
            RETURNING TRUE;
            """,
            new
            {
                GuildId = $"{member.GuildId}",
                UserId = $"{member.Id}",
                FirstJoinedAt = member.JoinedAt
            }
        );

        return result;
    }

    private class MemberAddedOrUpdatedDto
    {
        public DateTimeOffset? first_joined_at { get; set; }
    }

    public async ValueTask<MemberAddResult> AddNewMemberOrUpdateAsync(IGuildUser member)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var memberAddedOrUpdatedDto = await connection.QuerySingleAsync<MemberAddedOrUpdatedDto>(
            """
            INSERT INTO guilds.guild_members (guild_id, user_id, first_joined_at) VALUES (@GuildId, @UserId, @FirstJoinedAt)
            ON CONFLICT (guild_id, user_id) DO UPDATE SET
                alive = TRUE,
                first_joined_at = CASE
                    WHEN guild_members.first_joined_at IS NULL AND excluded.first_joined_at IS NOT NULL
                    THEN excluded.first_joined_at
                    ELSE guild_members.first_joined_at
                END
            RETURNING first_joined_at;
            """,
            new
            {
                GuildId = $"{member.GuildId}",
                UserId = $"{member.Id}",
                FirstJoinedAt = member.JoinedAt
            }
        );

        if (!memberAddedOrUpdatedDto.first_joined_at.HasValue || member.JoinedAt == memberAddedOrUpdatedDto.first_joined_at)
        {
            return new MemberAddResult();
        }
        else
        {
            return new RejoinedMemberAddResult(firstJoinedAt: memberAddedOrUpdatedDto.first_joined_at.Value);
        }
    }

    public async ValueTask UpdateMembersNotInGuildAsync(IGuild guild, IList<SnowflakeId> members)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            """
            UPDATE guilds.guild_members SET alive = FALSE
            WHERE guild_id = @GuildId AND user_id = ANY(@UserIds);
            """,
            new
            {
                GuildId = $"{guild.Id}",
                UserIds = members.Select(m => $"{m}").ToList(),
            }
        );
    }
}
