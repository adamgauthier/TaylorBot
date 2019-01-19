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
}

module.exports = UserDAO;