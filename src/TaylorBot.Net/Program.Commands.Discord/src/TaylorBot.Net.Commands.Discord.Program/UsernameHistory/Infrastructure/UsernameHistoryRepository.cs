using Dapper;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.UsernameHistory.Domain;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Discord.Program.UsernameHistory.Infrastructure
{
    public class UsernameHistoryRepository : IUsernameHistoryRepository
    {
        private readonly PostgresConnectionFactory _postgresConnectionFactory;

        public UsernameHistoryRepository(PostgresConnectionFactory postgresConnectionFactory)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
        }

        private record UsernameDto(string username, DateTimeOffset changed_at)
        {
            public UsernameDto() : this(default!, default) { }
        };

        public async ValueTask<IReadOnlyList<IUsernameHistoryRepository.UsernameChange>> GetUsernameHistoryFor(IUser user, int count)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            var usernames = await connection.QueryAsync<UsernameDto>(
                @"SELECT username, changed_at
                FROM users.usernames
                WHERE user_id = @UserId
                ORDER BY changed_at DESC
                LIMIT @MaxRows;",
                new
                {
                    UserId = user.Id.ToString(),
                    MaxRows = count
                }
            );

            return usernames.Select(name => new IUsernameHistoryRepository.UsernameChange(
                Username: name.username,
                ChangedAt: name.changed_at
            )).ToList();
        }
    }
}
