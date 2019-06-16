using Dapper;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Infrastructure.Options;
using TaylorBot.Net.Core.Infrastructure;
using Discord;
using TaylorBot.Net.EntityTracker.Domain.Member;
using System.Collections.Generic;
using System.Linq;

namespace TaylorBot.Net.EntityTracker.Infrastructure.Member
{
    public class MemberRepository : PostgresRepository, IMemberRepository
    {
        public MemberRepository(IOptionsMonitor<DatabaseConnectionOptions> optionsMonitor) : base(optionsMonitor)
        {
        }

        public async Task AddNewMemberAsync(IGuildUser member)
        {
            using (var connection = Connection)
            {
                connection.Open();

                await connection.ExecuteAsync(
                    "INSERT INTO guilds.guild_members (guild_id, user_id, first_joined_at) VALUES (@GuildId, @UserId, @FirstJoinedAt);",
                    new
                    {
                        GuildId = member.GuildId.ToString(),
                        UserId = member.Id.ToString(),
                        FirstJoinedAt = member.JoinedAt
                    }
                );
            }
        }

        public async Task<MemberAddResult> AddNewMemberIfNotAddedAsync(IGuildUser member)
        {
            using (var connection = Connection)
            {
                connection.Open();

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
        }

        public async Task SetMemberDeadAsync(IGuildUser member)
        {
            using (var connection = Connection)
            {
                connection.Open();

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

        public async Task UpdateDeadMembersAsync(IGuild guild, IEnumerable<IGuildUser> aliveMembers)
        {
            using (var connection = Connection)
            {
                connection.Open();

                await connection.ExecuteAsync(
                    "UPDATE guilds.guild_members SET alive = FALSE WHERE guild_id = @GuildId AND alive = TRUE AND NOT(user_id = ANY(@UserIds));",
                    new
                    {
                        GuildId = guild.Id.ToString(),
                        UserIds = aliveMembers.Select(m => m.Id.ToString()).ToList()
                    }
                );
            }
        }
    }
}
