using Dapper;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.UsernameHistory.Domain;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UsernameHistory.Infrastructure
{
    public class UsernameHistoryPostgresRepository : IUsernameHistoryRepository
    {
        private readonly PostgresConnectionFactory _postgresConnectionFactory;

        public UsernameHistoryPostgresRepository(PostgresConnectionFactory postgresConnectionFactory)
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

        public async ValueTask<bool> IsUsernameHistoryHiddenFor(IUser user)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            return await connection.QuerySingleOrDefaultAsync<bool>(
                @"SELECT is_hidden
                FROM users.username_history_configuration
                WHERE user_id = @UserId;",
                new
                {
                    UserId = user.Id.ToString()
                }
            );
        }

        public async ValueTask HideUsernameHistoryFor(IUser user)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                @"INSERT INTO users.username_history_configuration (user_id, is_hidden) VALUES (@UserId, @IsHidden)
                ON CONFLICT (user_id) DO UPDATE SET is_hidden = @IsHidden;",
                new
                {
                    UserId = user.Id.ToString(),
                    IsHidden = true
                }
            );
        }

        public async ValueTask UnhideUsernameHistoryFor(IUser user)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                @"UPDATE users.username_history_configuration
                SET is_hidden = FALSE
                WHERE user_id = @UserId;",
                new
                {
                    UserId = user.Id.ToString()
                }
            );
        }
    }
}
