'use strict';

const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

class GuildRepository {
    constructor(db) {
        this._db = db;
    }

    async getAll() {
        try {
            return await this._db.instance.any('SELECT * FROM guilds.guilds;');
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
            return await this._db.instance.oneOrNone(
                'SELECT * FROM guilds.guilds WHERE guild_id = $[guild_id];',
                databaseGuild
            );
        }
        catch (e) {
            Log.error(`Getting guild ${Format.guild(guild)}: ${e}`);
            throw e;
        }
    }

    async add(guild, discoveredAt) {
        try {
            return await this._db.instance.tx(async t => {
                const inserted = await t.one(
                    'INSERT INTO guilds.guilds (guild_id) VALUES ($1) RETURNING *;',
                    [guild.id]
                );
                await t.none(
                    'INSERT INTO guilds.guild_names (guild_id, guild_name, changed_at) VALUES ($1, $2, $3);',
                    [guild.id, guild.name, discoveredAt]
                );

                return inserted;
            });
        }
        catch (e) {
            Log.error(`Adding guild ${Format.guild(guild)}: ${e}`);
            throw e;
        }
    }

    async setPrefix(guild, prefix) {
        try {
            return await this._db.instance.oneOrNone(
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