﻿using Dapper;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.User;
using TaylorBot.Net.EntityTracker.Domain.User;

namespace TaylorBot.Net.EntityTracker.Infrastructure.User;

public class UserRepository(PostgresConnectionFactory postgresConnectionFactory) : IUserRepository
{
    private class UserAddedOrUpdatedDto
    {
        public bool was_inserted { get; set; }
        public bool username_changed { get; set; }
        public string? previous_username { get; set; }
    }

    public async ValueTask<UserAddedResult> AddNewUserAsync(DiscordUser user)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var userAddedOrUpdatedDto = await connection.QuerySingleAsync<UserAddedOrUpdatedDto>(
            """
            INSERT INTO users.users (user_id, is_bot, username, previous_username) VALUES (@UserId, @IsBot, @Username, NULL)
            ON CONFLICT (user_id) DO UPDATE SET
                previous_username = users.users.username,
                username = excluded.username
            RETURNING previous_username IS NULL AS was_inserted, previous_username IS DISTINCT FROM username AS username_changed, previous_username;
            """,
            new
            {
                UserId = $"{user.Id}",
                IsBot = user.IsBot,
                Username = user.Username,
            }
        );

        return new UserAddedResult(
            wasAdded: userAddedOrUpdatedDto.was_inserted,
            wasUsernameChanged: userAddedOrUpdatedDto.username_changed,
            previousUsername: userAddedOrUpdatedDto.previous_username
        );
    }
}
