'use strict';

const { Paths } = require('globalobjects');

const Log = require(Paths.Logger);
const Format = require(Paths.DiscordFormatter);

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

    async add(guild, discoveredAt) {
        try {
            return await this._db.instance.tx(async t => {
                const inserted = await t.one(
                    'INSERT INTO public.guilds (guild_id) VALUES ($1) RETURNING *;',
                    [guild.id]
                );
                await t.none(
                    'INSERT INTO public.guild_names (guild_id, guild_name, changed_at) VALUES ($1, $2, $3);',
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
            return await this._db.guilds.update(
                {
                    'guild_id': guild.id
                },
                {
                    'prefix': prefix
                },
                {
                    'single': true
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