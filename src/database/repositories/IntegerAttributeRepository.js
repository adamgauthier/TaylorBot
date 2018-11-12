'use strict';

const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

class IntegerAttributeRepository {
    constructor(db) {
        this._db = db;
    }

    async get(attributeId, user) {
        try {
            return await this._db.oneOrNone(
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

    async set(attributeId, user, value) {
        try {
            return await this._db.one(
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

    async clear(attributeId, user) {
        try {
            return await this._db.oneOrNone(
                `DELETE FROM attributes.integer_attributes
                WHERE attribute_id = $[attribute_id] AND user_id = $[user_id]
                RETURNING *;`,
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

    async getInGuild(attributeId, guild) {
        try {
            return await this._db.any(
                `SELECT * FROM attributes.integer_attributes
                WHERE user_id IN (
                   SELECT user_id
                   FROM guilds.guild_members
                   WHERE guild_id = $[guild_id] AND alive = TRUE
                )
                AND attribute_id = $[attribute_id];`,
                {
                    'guild_id': guild.id,
                    'attribute_id': attributeId
                }
            );
        }
        catch (e) {
            Log.error(`Getting attributes '${attributeId}' for guild ${Format.guild(guild)}: ${e}`);
            throw e;
        }
    }

    async getStatsInGuild(attributeId, guild) {
        try {
            return await this._db.one(
                `SELECT ROUND(AVG(integer_value), 2) AS avg, MIN(integer_value), MAX(integer_value), ROUND(MEDIAN(integer_value), 2) AS median
                FROM (
                    SELECT *
                    FROM attributes.integer_attributes
                    WHERE user_id IN (
                       SELECT user_id
                       FROM guilds.guild_members
                       WHERE guild_id = $[guild_id] AND alive = TRUE
                    )
                    AND attribute_id = $[attribute_id]
                ) AS guild_attributes;`,
                {
                    'guild_id': guild.id,
                    'attribute_id': attributeId
                }
            );
        }
        catch (e) {
            Log.error(`Getting attributes '${attributeId}' for guild ${Format.guild(guild)}: ${e}`);
            throw e;
        }
    }
}

module.exports = IntegerAttributeRepository;