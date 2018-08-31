'use strict';

const Log = require('../../tools/Logger.js');

class AttributeRepository {
    constructor(db) {
        this._db = db;
    }

    async getAll() {
        try {
            return await this._db.attributes.attributes.find();
        }
        catch (e) {
            Log.error(`Getting all attributes: ${e}`);
            throw e;
        }
    }

    async addAll(databaseAttributes) {
        try {
            return await this._db.attributes.attributes.insert(databaseAttributes);
        }
        catch (e) {
            Log.error(`Adding attributes: ${e}`);
            throw e;
        }
    }
}

module.exports = AttributeRepository;