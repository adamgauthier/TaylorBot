using Dapper;
using Discord;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Infrastructure;

public class MemberPostgresRepository : IMemberTrackingRepository
{
    private readonly PostgresConnectionFactory _postgresConnectionFactory;

    public MemberPostgresRepository(PostgresConnectionFactory postgresConnectionFactory)
    {
        _postgresConnectionFactory = postgresConnectionFactory;
    }

    public async ValueTask<bool> AddOrUpdateMemberAsync(IGuildUser member, DateTimeOffset? lastSpokeAt)
    {
        await using var connection = _postgresConnectionFactory.CreateConnection();

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
                GuildId = member.GuildId.ToString(),
                UserId = member.Id.ToString(),
                FirstJoinedAt = member.JoinedAt?.ToUniversalTime(),
                LastSpokeAt = lastSpokeAt?.ToUniversalTime()
            }
        );

        return experience == 0;
    }
}
