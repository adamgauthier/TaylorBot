using Dapper;
using Discord;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Infrastructure
{
    public class IgnoredUserPostgresRepository : IIgnoredUserRepository
    {
        private readonly PostgresConnectionFactory _postgresConnectionFactory;

        public IgnoredUserPostgresRepository(PostgresConnectionFactory postgresConnectionFactory)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
        }

        public async ValueTask<GetUserIgnoreUntilResult> InsertOrGetUserIgnoreUntilAsync(IUser user)
        {
            await using var connection = _postgresConnectionFactory.CreateConnection();

            var userAddedOrUpdatedDto = await connection.QuerySingleAsync<UserAddedOrUpdatedDto>(
                """
                INSERT INTO users.users (user_id, is_bot, username, previous_username) VALUES (@UserId, @IsBot, @Username, NULL)
                ON CONFLICT (user_id) DO UPDATE SET
                    previous_username = users.users.username,
                    username = excluded.username
                RETURNING
                    ignore_until, previous_username IS NULL AS was_inserted,
                    previous_username IS DISTINCT FROM username AS username_changed, previous_username;
                """,
                new
                {
                    UserId = user.Id.ToString(),
                    IsBot = user.IsBot,
                    Username = user.Username
                }
            );

            return new GetUserIgnoreUntilResult(
                IgnoreUntil: userAddedOrUpdatedDto.ignore_until,
                WasAdded: userAddedOrUpdatedDto.was_inserted,
                WasUsernameChanged: userAddedOrUpdatedDto.username_changed,
                PreviousUsername: userAddedOrUpdatedDto.previous_username
            );
        }

        private class UserAddedOrUpdatedDto
        {
            public DateTime ignore_until { get; set; }
            public bool was_inserted { get; set; }
            public bool username_changed { get; set; }
            public string? previous_username { get; set; }
        }

        public async ValueTask IgnoreUntilAsync(IUser user, DateTimeOffset until)
        {
            await using var connection = _postgresConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                @"UPDATE users.users SET ignore_until = @IgnoreUntil WHERE user_id = @UserId;",
                new
                {
                    UserId = user.Id.ToString(),
                    IgnoreUntil = until.ToUniversalTime(),
                }
            );
        }
    }
}
