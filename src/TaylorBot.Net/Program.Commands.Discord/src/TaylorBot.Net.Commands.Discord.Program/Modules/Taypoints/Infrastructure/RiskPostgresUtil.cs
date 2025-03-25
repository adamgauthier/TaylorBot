using Dapper;
using Npgsql;
using TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Domain;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Infrastructure;

public static class RiskPostgresUtil
{
    public record TaypointTransferDto(long invested_count, long final_count, long profit_count);

    private sealed record TaypointUpdateInfo(string Query, long AmountParam);

    public static async Task<TaypointTransferDto> WinRiskAsync(string payoutMultiplier, NpgsqlConnection connection, SnowflakeId userId, ITaypointAmount amount)
    {
        TaypointUpdateInfo updateInfo = amount switch
        {
            AbsoluteTaypointAmount absolute => new("LEAST(taypoint_count, @AmountParam)", absolute.Amount),
            RelativeTaypointAmount relative => new("FLOOR(taypoint_count / @AmountParam)::bigint", relative.Proportion),
            _ => throw new NotImplementedException(),
        };

        var transfer = await connection.QuerySingleAsync<TaypointTransferDto>(
            $"""
            UPDATE users.users AS u
            SET taypoint_count = taypoint_count + (invested_count * @PayoutMultiplier::double precision)
            FROM (
                SELECT user_id, {updateInfo.Query} AS invested_count, taypoint_count AS original_count
                FROM users.users WHERE user_id = @UserId FOR UPDATE
            ) AS old_u
            WHERE u.user_id = old_u.user_id
            RETURNING
            old_u.invested_count
            ,u.taypoint_count AS final_count
            ,u.taypoint_count - old_u.original_count AS profit_count;
            """,
            new
            {
                UserId = $"{userId}",
                updateInfo.AmountParam,
                PayoutMultiplier = payoutMultiplier,
            }
        );
        return transfer;
    }

    public static async Task<TaypointTransferDto> LoseRiskAsync(NpgsqlConnection connection, SnowflakeId userId, ITaypointAmount amount)
    {
        TaypointUpdateInfo updateInfo = amount switch
        {
            AbsoluteTaypointAmount absolute => new("LEAST(taypoint_count, @AmountParam)", absolute.Amount),
            RelativeTaypointAmount relative => new("FLOOR(taypoint_count / @AmountParam)::bigint", relative.Proportion),
            _ => throw new NotImplementedException(),
        };

        var transfer = await connection.QuerySingleAsync<TaypointTransferDto>(
            $"""
            UPDATE users.users AS u
            SET taypoint_count = GREATEST(0, taypoint_count - invested_count)
            FROM (
                SELECT user_id, {updateInfo.Query} AS invested_count, taypoint_count AS original_count
                FROM users.users WHERE user_id = @UserId FOR UPDATE
            ) AS old_u
            WHERE u.user_id = old_u.user_id
            RETURNING
            old_u.invested_count
            ,u.taypoint_count AS final_count
            ,u.taypoint_count - old_u.original_count AS profit_count;
            """,
            new
            {
                UserId = $"{userId}",
                updateInfo.AmountParam,
            }
        );
        return transfer;
    }
}
