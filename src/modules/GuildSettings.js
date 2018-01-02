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

    async addGuild(guild) {
        Log.verbose(`Adding guild ${Format.guild(guild)}.`);

        let databaseGuild = await this.database.getGuild(guild);
        if (!databaseGuild) {
            databaseGuild = await this.database.addGuild(guild);
            Log.verbose(`Added guild ${Format.guild(guild)} to database.`);
        }

        if (this.has(databaseGuild.guild_id)) {
            Log.warn(`Adding guild ${Format.guild(guild)}, already cached, overwriting with database guild.`);
        }

        this.cacheGuild(databaseGuild);
    }
}

module.exports = GuildSettings;