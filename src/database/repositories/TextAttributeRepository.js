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
}

module.exports = TextAttributeRepository;