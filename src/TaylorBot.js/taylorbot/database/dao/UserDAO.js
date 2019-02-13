'use strict';

class UserDAO {
    addTaypointCount(queryable, usersTo, count) {
        return queryable.many(
            'UPDATE users.users SET taypoint_count = taypoint_count + $[points_to_add] WHERE user_id IN ($[user_ids:csv]) RETURNING taypoint_count;',
            {
                points_to_add: count,
                user_ids: usersTo.map(user => user.id)
            }
        );
    }

    loseBet(queryable, userId, betAmount) {
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

    winBet(queryable, userId, betAmount, payoutMultiplier) {
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

module.exports = UserDAO;