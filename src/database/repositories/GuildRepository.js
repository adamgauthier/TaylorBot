'use strict';

const { GlobalPaths } = require('globalobjects');

const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);

class GuildRepository {
    constructor(db) {
        this._db = db;
    }

    async getAll() {
        try {
            return await this._db.guilds.find();
        }
        catch (e) {
            Log.error(`Getting all guilds: ${e}`);
            throw e;
        }
    }

    mapGuildToDatabase(guild) {
        return {
            'guild_id': guild.id
        };
    }

    async get(guild) {
        const databaseGuild = this.mapGuildToDatabase(guild);
        try {
            return await this._db.guilds.findOne(databaseGuild);
        }
        catch (e) {
            Log.error(`Getting guild ${Format.guild(guild)}: ${e}`);
            throw e;
        }
    }

    async add(guild) {
        const databaseGuild = this.mapGuildToDatabase(guild);
        try {
            return await this._db.guilds.insert(databaseGuild);
        }
        catch (e) {
            Log.error(`Adding guild ${Format.guild(guild)}: ${e}`);
            throw e;
        }
    }
}

module.exports = GuildRepository;