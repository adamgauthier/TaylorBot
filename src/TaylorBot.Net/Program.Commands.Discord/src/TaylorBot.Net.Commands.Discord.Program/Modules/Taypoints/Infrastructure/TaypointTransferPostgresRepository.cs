using Dapper;
using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Domain;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Infrastructure;

public class TaypointTransferPostgresRepository : ITaypointTransferRepository
{
    private readonly PostgresConnectionFactory _postgresConnectionFactory;

    public TaypointTransferPostgresRepository(PostgresConnectionFactory postgresConnectionFactory)
    {
        _postgresConnectionFactory = postgresConnectionFactory;
    }

    public async ValueTask<TransferResult> TransferTaypointsAsync(IUser from, IUser to, int taypointCount)
    {
        await using var connection = _postgresConnectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        var removedTaypoint = await connection.QuerySingleAsync<RemoveTaypointDto>(
            """
            UPDATE users.users AS u
            SET taypoint_count = GREATEST(0, taypoint_count - @PointsToRemove)
            FROM (
                SELECT user_id, taypoint_count AS original_count
                FROM users.users WHERE user_id = @SenderId FOR UPDATE
            ) AS old_u
            WHERE u.user_id = old_u.user_id
            RETURNING old_u.original_count, (old_u.original_count - u.taypoint_count) AS gifted_count
            """,
            new
            {
                PointsToRemove = taypointCount,
                SenderId = $"{from.Id}",
            }
        );

        long receiverNewCount = 0;

        if (removedTaypoint.gifted_count > 0)
        {
            receiverNewCount = await connection.QuerySingleAsync<long>(
                "UPDATE users.users SET taypoint_count = taypoint_count + @PointsToGift WHERE user_id = @ReceiverId RETURNING taypoint_count;",
                new
                {
                    PointsToGift = taypointCount,
                    ReceiverId = $"{to.Id}",
                }
            );
        }

        transaction.Commit();
        return new TransferResult(removedTaypoint.original_count, removedTaypoint.gifted_count, receiverNewCount);
    }

    private record RemoveTaypointDto(long original_count, long gifted_count);
}

