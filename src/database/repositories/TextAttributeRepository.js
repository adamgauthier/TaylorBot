'use strict';

const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

class TextAttributeRepository {
    constructor(db) {
        this._db = db;
    }

    async get(attributeId, user) {
        try {
            return await this._db.attributes.text_attributes.findOne({
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
                'INSERT INTO attributes.text_attributes (attribute_id, user_id, attribute_value)',
                'VALUES (${attribute_id}, ${user_id}, ${attribute_value})',
                'ON CONFLICT (attribute_id, user_id) DO UPDATE',
                '  SET attribute_value = excluded.attribute_value',
                'RETURNING *;'
            ].join('\n'),
            {
                'user_id': user.id,
                'attribute_id': attributeId,
                'attribute_value': value
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
                'DELETE FROM attributes.text_attributes',
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
}

module.exports = TextAttributeRepository;