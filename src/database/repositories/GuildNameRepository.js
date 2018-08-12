'use strict';

const { Paths } = require('globalobjects');

const Log = require(Paths.Logger);
const Format = require(Paths.DiscordFormatter);

class GuildNameRepository {
    constructor(db) {
        this._db = db;
    }

    async getAllLatest() {
        try {
            return await this._db.guild_names.getLatestGuildNames();
        }
        catch (e) {
            Log.error(`Getting all guild names: ${e}`);
            throw e;
        }
    }

    async getLatest(guild) {
        try {
            return await this._db.guild_names.getLatestGuildName(
                {
                    'guild_id': guild.id
                },
                {
                    'single': true
                }
            );
        }
        catch (e) {
            Log.error(`Getting latest guild name for guild ${Format.guild(guild)}: ${e}`);
            throw e;
        }
    }

    mapGuildToGuildNamesDatabase(guild, changedAt) {
        return {
            'guild_id': guild.id,
            'guild_name': guild.name,
            'changed_at': changedAt
        };
    }

    async add(guild, changedAt) {
        const databaseGuildName = this.mapGuildToGuildNamesDatabase(guild, changedAt);
        try {
            return await this._db.guilds.guild_names.insert(databaseGuildName);
        }
        catch (e) {
            Log.error(`Adding guild name for ${Format.guild(guild)}: ${e}`);
            throw e;
        }
    }

    async getHistory(guild, limit) {
        try {
            return await this._db.guild_names.getGuildNameHistory(
                {
                    'guild_id': guild.id,
                    'max_rows': limit
                }
            );
        }
        catch (e) {
            Log.error(`Getting guild name history for ${Format.guild(guild)}: ${e}`);
            throw e;
        }
    }
}

module.exports = GuildNameRepository;