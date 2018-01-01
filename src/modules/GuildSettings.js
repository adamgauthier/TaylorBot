'use strict';

const path = require('path');

const GlobalPaths = require(path.join(__dirname, '..', 'GlobalPaths'));

const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);

class GuildSettings extends Map {
    constructor(database) {
        super();
        this.database = database;
    }

    async load() {
        const guilds = await this.database.getAllGuilds();
        guilds.forEach(g => this.cacheGuild(g));
    }

    cacheGuild(databaseGuild) {
        this.set(databaseGuild.guild_id, {
            'prefix': databaseGuild.prefix
        });
    }

    mapGuildToDatabase(guild) {
        return {
            'guild_id': guild.id
        };
    }

    async addGuild(guild) {
        Log.verbose(`Adding guild ${Format.formatGuild(guild)}.`);
        const databaseGuild = this.mapGuildToDatabase(guild);

        let savedGuild = await this.database.updateGuild(databaseGuild);
        if (!savedGuild) {
            Log.verbose(`Adding guild ${Format.formatGuild(guild)}, not in database, attempting to insert.`);
            savedGuild = await this.database.addGuild(databaseGuild);
        }            

        if (this.has(savedGuild.guild_id)) {
            Log.warn(`Adding guild ${Format.formatGuild(guild)}, already cached, overwriting with database guild.`);
        }

        this.cacheGuild(savedGuild);
    }
}

module.exports = GuildSettings;