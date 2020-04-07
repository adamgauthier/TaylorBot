'use strict';

const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

class GuildRepository {
    constructor(db) {
        this._db = db;
    }

    async getAll() {
        try {
            return await this._db.any('SELECT * FROM guilds.guilds;');
        }
        catch (e) {
            Log.error(`Getting all guilds: ${e}`);
            throw e;
        }
    }

    mapGuildToDatabase(guild) {
        return {
            guild_id: guild.id,
            guild_name: guild.name
        };
    }

    async get(guild) {
        const databaseGuild = this.mapGuildToDatabase(guild);
        try {
            return await this._db.oneOrNone(
                'SELECT * FROM guilds.guilds WHERE guild_id = $[guild_id];',
                databaseGuild
            );
        }
        catch (e) {
            Log.error(`Getting guild ${Format.guild(guild)}: ${e}`);
            throw e;
        }
    }

    async getPrefix(guild) {
        const databaseGuild = this.mapGuildToDatabase(guild);
        try {
            return await this._db.oneOrNone(
                `INSERT INTO guilds.guilds (guild_id, guild_name, previous_guild_name) VALUES ($[guild_id], $[guild_name], NULL)
                ON CONFLICT (guild_id) DO UPDATE SET
                    previous_guild_name = guilds.guilds.guild_name,
                    guild_name = excluded.guild_name
                RETURNING prefix;`,
                databaseGuild
            );
        }
        catch (e) {
            Log.error(`Getting guild prefix ${Format.guild(guild)}: ${e}`);
            throw e;
        }
    }

    async setPrefix(guild, prefix) {
        try {
            return await this._db.oneOrNone(
                `UPDATE guilds.guilds SET prefix = $[prefix]
                WHERE guild_id = $[guild_id]
                RETURNING *;`,
                {
                    'prefix': prefix,
                    'guild_id': guild.id
                }
            );
        }
        catch (e) {
            Log.error(`Setting guild prefix for ${Format.guild(guild)} to '${prefix}': ${e}`);
            throw e;
        }
    }
}

module.exports = GuildRepository;
