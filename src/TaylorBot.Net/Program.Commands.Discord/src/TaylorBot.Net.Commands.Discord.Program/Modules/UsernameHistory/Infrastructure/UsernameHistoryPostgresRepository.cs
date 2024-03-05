using Dapper;
using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.UsernameHistory.Domain;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UsernameHistory.Infrastructure;

public class UsernameHistoryPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : IUsernameHistoryRepository
{
    private record UsernameDto(string username, DateTime changed_at);

    public async ValueTask<IReadOnlyList<UsernameChange>> GetUsernameHistoryFor(IUser user, int count)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var usernames = await connection.QueryAsync<UsernameDto>(
            """
            SELECT username, changed_at
            FROM users.usernames
            WHERE user_id = @UserId
            ORDER BY changed_at DESC
            LIMIT @MaxRows;
            """,
            new
            {
                UserId = $"{user.Id}",
                MaxRows = count,
            }
        );

        return usernames.Select(name => new UsernameChange(
            Username: name.username,
            ChangedAt: name.changed_at
        )).ToList();
    }

    public async ValueTask<bool> IsUsernameHistoryHiddenFor(IUser user)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<bool>(
            """
            SELECT is_hidden
            FROM users.username_history_configuration
            WHERE user_id = @UserId;
            """,
            new
            {
                UserId = $"{user.Id}",
            }
        );
    }

    public async ValueTask HideUsernameHistoryFor(IUser user)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            """
            INSERT INTO users.username_history_configuration (user_id, is_hidden)
            VALUES (@UserId, @IsHidden)
            ON CONFLICT (user_id) DO UPDATE SET is_hidden = @IsHidden;
            """,
            new
            {
                UserId = $"{user.Id}",
                IsHidden = true,
            }
        );
    }

    public async ValueTask UnhideUsernameHistoryFor(IUser user)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            """
            UPDATE users.username_history_configuration
            SET is_hidden = FALSE
            WHERE user_id = @UserId;
            """,
            new
            {
                UserId = $"{user.Id}",
            }
        );
    }
}
