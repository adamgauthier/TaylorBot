import Log = require('../../tools/Logger.js');
import Format = require('../../modules/DiscordFormatter.js');
import * as pgPromise from 'pg-promise';
import { Guild, User } from 'discord.js';

export class TextAttributeRepository {
    readonly #db: pgPromise.IDatabase<unknown>;

    constructor(db: pgPromise.IDatabase<unknown>) {
        this.#db = db;
    }

    async get(attributeId: string, user: User): Promise<{ attribute_id: string; user_id: string; attribute_value: string } | null> {
        try {
            return await this.#db.oneOrNone(
                'SELECT * FROM attributes.text_attributes WHERE user_id = $[user_id] AND attribute_id = $[attribute_id];',
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

    async set(attributeId: string, user: User, value: string): Promise<{ attribute_id: string; user_id: string; attribute_value: string }> {
        try {
            return await this.#db.one(
                `INSERT INTO attributes.text_attributes (attribute_id, user_id, attribute_value)
                VALUES ($[attribute_id], $[user_id], $[attribute_value])
                ON CONFLICT (attribute_id, user_id) DO UPDATE
                  SET attribute_value = excluded.attribute_value
                RETURNING *;`,
                {
                    'user_id': user.id,
                    'attribute_id': attributeId,
                    'attribute_value': value
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
                `DELETE FROM attributes.text_attributes
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

    async listInGuild(attributeId: string, guild: Guild, count: number): Promise<{ attribute_id: string; user_id: string; attribute_value: string }[]> {
        try {
            return await this.#db.any(
                `SELECT * FROM attributes.text_attributes
                WHERE user_id IN (
                   SELECT user_id
                   FROM guilds.guild_members
                   WHERE guild_id = $[guild_id] AND alive = TRUE
                )
                AND attribute_id = $[attribute_id]
                LIMIT $[count];`,
                {
                    'guild_id': guild.id,
                    'attribute_id': attributeId,
                    count
                }
            );
        }
        catch (e) {
            Log.error(`Listing attribute '${attributeId}' for guild ${Format.guild(guild)}: ${e}`);
            throw e;
        }
    }
}
