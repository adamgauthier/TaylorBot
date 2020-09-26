import { User } from "discord.js";
import * as pgPromise from 'pg-promise';
import { TaypointAmount } from '../../modules/points/TaypointAmount';

export class UserDAO {
    addTaypointCount(queryable: pgPromise.IBaseProtocol<unknown>, usersTo: User[], count: number): Promise<{ taypoint_count: string }[]> {
        return queryable.many(
            'UPDATE users.users SET taypoint_count = taypoint_count + $[points_to_add] WHERE user_id IN ($[user_ids:csv]) RETURNING taypoint_count;',
            {
                points_to_add: count,
                user_ids: usersTo.map(user => user.id)
            }
        );
    }

    loseBet(queryable: pgPromise.IBaseProtocol<unknown>, userId: string, betAmount: TaypointAmount): Promise<{
        user_id: string;
        gambled_count: string;
        original_count: string;
        final_count: string;
        lost_count: string;
    }> {
        const toRemove = betAmount.isRelative ?
            { query: 'FLOOR(taypoint_count / $[points_divisor])::bigint', params: { points_divisor: betAmount.divisor } } :
            { query: 'LEAST(taypoint_count, $[gamble_points])', params: { gamble_points: betAmount.count } };

        return queryable.one(
            `UPDATE users.users AS u
            SET taypoint_count = GREATEST(0, taypoint_count - ${toRemove.query})
            FROM (
                SELECT user_id, ${toRemove.query} AS gambled_count, taypoint_count AS original_count
                FROM users.users WHERE user_id = $[user_id] FOR UPDATE
            ) AS old_u
            WHERE u.user_id = old_u.user_id
            RETURNING u.user_id, old_u.gambled_count, old_u.original_count,
            u.taypoint_count AS final_count, old_u.original_count - u.taypoint_count AS lost_count;`,
            {
                ...toRemove.params,
                user_id: userId
            }
        );
    }

    winBet(queryable: pgPromise.IBaseProtocol<unknown>, userId: string, betAmount: TaypointAmount, payoutMultiplier: string): Promise<{
        user_id: string;
        gambled_count: string;
        original_count: string;
        final_count: string;
        payout_count: string;
    }> {
        const toAdd = betAmount.isRelative ?
            { query: 'FLOOR(taypoint_count / $[points_divisor])::bigint', params: { points_divisor: betAmount.divisor } } :
            { query: 'LEAST(taypoint_count, $[gamble_points])', params: { gamble_points: betAmount.count } };

        return queryable.one(
            `UPDATE users.users AS u
            SET taypoint_count = taypoint_count + (${toAdd.query} * $[payout_multiplier]::double precision)
            FROM (
                SELECT user_id, ${toAdd.query} AS gambled_count, taypoint_count AS original_count
                FROM users.users WHERE user_id = $[user_id] FOR UPDATE
            ) AS old_u
            WHERE u.user_id = old_u.user_id
            RETURNING u.user_id, old_u.gambled_count, old_u.original_count,
            u.taypoint_count AS final_count, u.taypoint_count - old_u.original_count AS payout_count;`,
            {
                ...toAdd.params,
                payout_multiplier: payoutMultiplier,
                user_id: userId
            }
        );
    }
}
