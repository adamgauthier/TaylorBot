'use strict';

const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

class GuildNameRepository {
    constructor(db) {
        this._db = db;
    }

    async getAllLatest() {
        try {
            return await this._db.instance.any(
                `SELECT g.guild_name, g.guild_id
                FROM (
                   SELECT guild_id, MAX(changed_at) AS max_changed_at
                   FROM guilds.guild_names
                   GROUP BY guild_id
                ) AS maxed
                JOIN guilds.guild_names AS g ON g.guild_id = maxed.guild_id AND g.changed_at = maxed.max_changed_at;`
            );
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
            return await this._db.instance.any(
                `SELECT guild_name, changed_at
                FROM guilds.guild_names
                WHERE guild_id = $[guild_id]
                ORDER BY changed_at DESC
                LIMIT $[max_rows];`,
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