'use strict';

const { Paths } = require('globalobjects');

const Log = require(Paths.Logger);
const Format = require(Paths.DiscordFormatter);

class UserRepository {
    constructor(db) {
        this._db = db;
    }

    async getAll() {
        try {
            return await this._db.users.find({}, {
                fields: ['user_id', 'ignore_until']
            });
        }
        catch (e) {
            Log.error(`Getting all users: ${e}`);
            throw e;
        }
    }

    mapUserToDatabase(user) {
        return {
            'user_id': user.id
        };
    }

    async get(user) {
        const databaseUser = this.mapUserToDatabase(user);
        try {
            return await this._db.users.findOne(databaseUser);
        }
        catch (e) {
            Log.error(`Getting user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }

    async add(user) {
        const databaseUser = this.mapUserToDatabase(user);
        try {
            return await this._db.users.insert(databaseUser);
        }
        catch (e) {
            Log.error(`Adding user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }
}

module.exports = UserRepository;