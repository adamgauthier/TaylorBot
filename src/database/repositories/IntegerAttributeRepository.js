'use strict';

const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

class IntegerAttributeRepository {
    constructor(db) {
        this._db = db;
    }

    async get(attributeId, user) {
        try {
            return await this._db.instance.oneOrNone([
                'SELECT * FROM attributes.integer_attributes',
                'WHERE attribute_id = ${attribute_id} AND user_id = ${user_id};'
            ].join('\n'),
            {
                'user_id': user.id,
                'attribute_id': attributeId
            });
        }
        catch (e) {
            Log.error(`Getting attribute '${attributeId}' for user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }

    async set(attributeId, user, value) {
        try {
            return await this._db.instance.one([
                'INSERT INTO attributes.integer_attributes (attribute_id, user_id, integer_value)',
                'VALUES (${attribute_id}, ${user_id}, ${integer_value})',
                'ON CONFLICT (attribute_id, user_id) DO UPDATE',
                '  SET integer_value = excluded.integer_value',
                'RETURNING *;'
            ].join('\n'),
            {
                'user_id': user.id,
                'attribute_id': attributeId,
                'integer_value': value
            });
        }
        catch (e) {
            Log.error(`Setting attribute '${attributeId}' to '${value}' for user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }

    async clear(attributeId, user) {
        try {
            return await this._db.instance.oneOrNone([
                'DELETE FROM attributes.integer_attributes',
                'WHERE attribute_id = ${attribute_id} AND user_id = ${user_id}',
                'RETURNING *;'
            ].join('\n'),
            {
                'user_id': user.id,
                'attribute_id': attributeId
            });
        }
        catch (e) {
            Log.error(`Clearing attribute '${attributeId}' for user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }

    async getInGuild(attributeId, guild) {
        try {
            return await this._db.instance.any([
                'SELECT * FROM attributes.integer_attributes',
                'WHERE user_id IN (',
                '   SELECT user_id',
                '   FROM guilds.guild_members',
                '   WHERE guild_id = ${guild_id}',
                ')',
                'AND attribute_id = ${attribute_id};'
            ].join('\n'),
            {
                'guild_id': guild.id,
                'attribute_id': attributeId
            });
        }
        catch (e) {
            Log.error(`Getting attributes '${attributeId}' for guild ${Format.guild(guild)}: ${e}`);
            throw e;
        }
    }
}

module.exports = IntegerAttributeRepository;