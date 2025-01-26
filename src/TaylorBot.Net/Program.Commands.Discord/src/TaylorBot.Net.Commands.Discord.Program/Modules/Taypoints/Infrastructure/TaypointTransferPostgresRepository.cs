using Dapper;
using TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Domain;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Infrastructure.Taypoints;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Infrastructure;

public class TaypointTransferPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : ITaypointTransferRepository
{
    public async ValueTask<TransferResult> TransferTaypointsAsync(DiscordUser from, IReadOnlyList<DiscordUser> to, ITaypointAmount amount)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        var removeInfo = amount is AbsoluteTaypointAmount absolute
            ? new { ToRemove = "@AmountParam", AmountParam = absolute.Amount }
            : new { ToRemove = "FLOOR(taypoint_count / @AmountParam)::bigint", AmountParam = (long)((RelativeTaypointAmount)amount).Proportion };

        var removedTaypoint = await connection.QuerySingleAsync<RemoveTaypointDto>(
            $"""
            UPDATE users.users AS u
            SET taypoint_count = GREATEST(0, taypoint_count - {removeInfo.ToRemove})
            FROM (
                SELECT user_id, taypoint_count AS original_count
                FROM users.users WHERE user_id = @SenderId FOR UPDATE
            ) AS old_u
            WHERE u.user_id = old_u.user_id
            RETURNING old_u.original_count, (old_u.original_count - u.taypoint_count) AS gifted_count
            """,
            new
            {
                AmountParam = removeInfo.AmountParam,
                SenderId = $"{from.Id}",
            }
        );

        var baseGiftCount = removedTaypoint.gifted_count / to.Count;
        List<RecipientUser> recipientUsers = [
            // First recipient gets all the unevenly divided points
            new(to[0], baseGiftCount + removedTaypoint.gifted_count % to.Count),
            .. to.Skip(1).Select(user => new RecipientUser(user, baseGiftCount))
        ];

        List<TransferResult.Recipient> recipients = [];

        foreach (var recipient in recipientUsers.Where(r => r.Amount > 0))
        {
            var addResult = await TaypointPostgresUtil.AddTaypointsReturningAsync(connection, recipient.User.Id, pointsToAdd: recipient.Amount);
            recipients.Add(new(recipient.User.Id, recipient.Amount, addResult.taypoint_count));
        }

        transaction.Commit();
        return new TransferResult(removedTaypoint.original_count, removedTaypoint.gifted_count, recipients);
    }

    private record RemoveTaypointDto(long original_count, long gifted_count);

    private record RecipientUser(DiscordUser User, long Amount);
}
