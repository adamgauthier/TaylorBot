using Dapper;
using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Infrastructure
{
    public class MemberPostgresRepository : IMemberRepository
    {
        private readonly PostgresConnectionFactory _postgresConnectionFactory;

        public MemberPostgresRepository(PostgresConnectionFactory postgresConnectionFactory)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
        }

        public async Task<bool> AddOrUpdateMemberAsync(IGuildUser member)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            var experience = await connection.QuerySingleAsync<long>(
                @"INSERT INTO guilds.guild_members (guild_id, user_id, first_joined_at) VALUES (@GuildId, @UserId, @FirstJoinedAt)
                ON CONFLICT (guild_id, user_id) DO UPDATE SET
                    alive = TRUE,
                    first_joined_at = CASE
                        WHEN guild_members.first_joined_at IS NULL AND excluded.first_joined_at IS NOT NULL
                        THEN excluded.first_joined_at
                        ELSE guild_members.first_joined_at
                    END,
                    experience = guild_members.experience + 1
                RETURNING experience;",
                new
                {
                    GuildId = member.GuildId.ToString(),
                    UserId = member.Id.ToString(),
                    FirstJoinedAt = member.JoinedAt
                }
            );

            return experience == 0;
        }
    }
}
