import Log = require('../../tools/Logger.js');
import Format = require('../../modules/DiscordFormatter.js');
import * as pgPromise from 'pg-promise';
import { User } from 'discord.js';

export class IntegerAttributeRepository {
    readonly #db: pgPromise.IDatabase<unknown>;

    constructor(db: pgPromise.IDatabase<unknown>) {
        this.#db = db;
    }

    async get(attributeId: string, user: User): Promise<{ attribute_id: string; user_id: string; integer_value: number } | null> {
        try {
            return await this.#db.oneOrNone(
                `SELECT * FROM attributes.integer_attributes
                WHERE attribute_id = $[attribute_id] AND user_id = $[user_id];`,
                {
                    'user_id': user.id,
                    'attribute_id': attributeId
                }
            );
        }
        catch (e) {
            Log.error(`Getting attribute '${attributeId}' for user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }

    async set(attributeId: string, user: User, value: number): Promise<{ attribute_id: string; user_id: string; integer_value: number }> {
        try {
            return await this.#db.one(
                `INSERT INTO attributes.integer_attributes (attribute_id, user_id, integer_value)
                VALUES ($[attribute_id], $[user_id], $[integer_value])
                ON CONFLICT (attribute_id, user_id) DO UPDATE
                  SET integer_value = excluded.integer_value
                RETURNING *;`,
                {
                    'user_id': user.id,
                    'attribute_id': attributeId,
                    'integer_value': value
                }
            );
        }
        catch (e) {
            Log.error(`Setting attribute '${attributeId}' to '${value}' for user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }

    async clear(attributeId: string, user: User): Promise<void> {
        try {
            await this.#db.none(
                `DELETE FROM attributes.integer_attributes
                WHERE attribute_id = $[attribute_id] AND user_id = $[user_id];`,
                {
                    'user_id': user.id,
                    'attribute_id': attributeId
                }
            );
        }
        catch (e) {
            Log.error(`Clearing attribute '${attributeId}' for user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }
}
