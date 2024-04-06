using Dapper;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Infrastructure;

public class MemberPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : IMemberTrackingRepository
{
    public async ValueTask<bool> AddOrUpdateMemberAsync(DiscordMember member, DateTimeOffset? lastSpokeAt)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var experience = await connection.QuerySingleAsync<long>(
            """
            INSERT INTO guilds.guild_members (guild_id, user_id, first_joined_at, last_spoke_at) VALUES (@GuildId, @UserId, @FirstJoinedAt, @LastSpokeAt)
            ON CONFLICT (guild_id, user_id) DO UPDATE SET
                alive = TRUE,
                first_joined_at = CASE
                    WHEN guild_members.first_joined_at IS NULL AND excluded.first_joined_at IS NOT NULL
                    THEN excluded.first_joined_at
                    ELSE guild_members.first_joined_at
                END,
                experience = guild_members.experience + 1,
                last_spoke_at = CASE
                    WHEN excluded.last_spoke_at IS NULL
                    THEN guild_members.last_spoke_at
                    ELSE excluded.last_spoke_at
                END
            RETURNING experience;
            """,
            new
            {
                GuildId = $"{member.Member.GuildId}",
                UserId = $"{member.User.Id}",
                FirstJoinedAt = member.Member.JoinedAt?.ToUniversalTime(),
                LastSpokeAt = lastSpokeAt?.ToUniversalTime(),
            }
        );

        return experience == 0;
    }
}
