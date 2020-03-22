'use strict';

const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

class GuildNameRepository {
    constructor(db) {
        this._db = db;
    }

    mapGuildToGuildNamesDatabase(guild) {
        return {
            'guild_id': guild.id,
            'guild_name': guild.name
        };
    }

    async add(guild) {
        const databaseGuildName = this.mapGuildToGuildNamesDatabase(guild);
        try {
            return await this._db.none(
                'INSERT INTO guilds.guild_names (guild_id, guild_name) VALUES ($[guild_id], $[guild_name]);',
                databaseGuildName
            );
        }
        catch (e) {
            Log.error(`Adding guild name for ${Format.guild(guild)}: ${e}`);
            throw e;
        }
    }

    async getHistory(guild, limit) {
        try {
            return await this._db.any(
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