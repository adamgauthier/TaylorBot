using Dapper;
using TaylorBot.Net.Commands.Discord.Program.Modules.UsernameHistory.Domain;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UsernameHistory.Infrastructure;

public class UsernameHistoryPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : IUsernameHistoryRepository
{
    private sealed record UsernameDto(string username, DateTime changed_at);

    public async ValueTask<IReadOnlyList<UsernameChange>> GetUsernameHistoryFor(DiscordUser user, int count)
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

        return [.. usernames.Select(name => new UsernameChange(
            Username: name.username,
            ChangedAt: name.changed_at
        ))];
    }

    public async ValueTask<bool> IsUsernameHistoryHiddenFor(DiscordUser user)
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

    public async ValueTask HideUsernameHistoryFor(DiscordUser user)
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

    public async ValueTask UnhideUsernameHistoryFor(DiscordUser user)
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
