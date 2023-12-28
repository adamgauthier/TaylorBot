import { User } from 'discord.js';
import * as pgPromise from 'pg-promise';

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
}
