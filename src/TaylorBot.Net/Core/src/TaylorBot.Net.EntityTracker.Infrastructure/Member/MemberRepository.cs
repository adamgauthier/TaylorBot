using Dapper;
using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.EntityTracker.Domain.Member;

namespace TaylorBot.Net.EntityTracker.Infrastructure.Member
{
    public class MemberRepository : IMemberRepository
    {
        private readonly PostgresConnectionFactory _postgresConnectionFactory;

        public MemberRepository(PostgresConnectionFactory postgresConnectionFactory)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
        }

        public async ValueTask<bool> AddNewMemberAsync(IGuildUser member)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            var result = await connection.QuerySingleOrDefaultAsync<bool>(
                @"INSERT INTO guilds.guild_members (guild_id, user_id, first_joined_at) VALUES (@GuildId, @UserId, @FirstJoinedAt)
                ON CONFLICT (guild_id, user_id) DO NOTHING
                RETURNING TRUE;",
                new
                {
                    GuildId = member.GuildId.ToString(),
                    UserId = member.Id.ToString(),
                    FirstJoinedAt = member.JoinedAt
                }
            );

            return result;
        }

        public async ValueTask<MemberAddResult> AddNewMemberOrUpdateAsync(IGuildUser member)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            var memberAddedOrUpdatedDto = await connection.QuerySingleAsync<MemberAddedOrUpdatedDto>(
                @"INSERT INTO guilds.guild_members (guild_id, user_id, first_joined_at) VALUES (@GuildId, @UserId, @FirstJoinedAt)
                ON CONFLICT (guild_id, user_id) DO UPDATE SET
                    alive = TRUE,
                    first_joined_at = CASE
                        WHEN guild_members.first_joined_at IS NULL AND excluded.first_joined_at IS NOT NULL
                        THEN excluded.first_joined_at
                        ELSE guild_members.first_joined_at
                    END
                RETURNING first_joined_at;",
                new
                {
                    GuildId = member.GuildId.ToString(),
                    UserId = member.Id.ToString(),
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

        public async ValueTask SetMemberDeadAsync(IGuildUser member)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                "UPDATE guilds.guild_members SET alive = FALSE WHERE guild_id = @GuildId AND user_id = @UserId;",
                new
                {
                    GuildId = member.GuildId.ToString(),
                    UserId = member.Id.ToString()
                }
            );
        }
    }
}
