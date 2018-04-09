'use strict';

const { GlobalPaths } = require('globalobjects');

const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);

class GuildRegistry extends Map {
    constructor(client) {
        super();
        this.client = client;
    }

    async load() {
        const guilds = await this.client.database.getAllGuilds();
        guilds.forEach(g => this.cacheGuild(g));
    }

    cacheGuild(databaseGuild) {
        this.set(databaseGuild.guild_id, {
            'prefix': databaseGuild.prefix,
            'roleGroups': {}
        });
    }

    async addGuild(guild) {
        Log.verbose(`Adding guild ${Format.guild(guild)}.`);

        const { database } = this.client;

        let databaseGuild = await database.getGuild(guild);
        if (!databaseGuild) {
            databaseGuild = await database.addGuild(guild);
            Log.verbose(`Added guild ${Format.guild(guild)} to database.`);
        }

        if (this.has(databaseGuild.guild_id)) {
            Log.warn(`Adding guild ${Format.guild(guild)}, already cached, overwriting with database guild.`);
        }

        this.cacheGuild(databaseGuild);
    }

    async changePrefix(guild, prefix) {
        const cachedGuild = this.get(guild.id);

        if (!cachedGuild)
            throw new Error(`Could not change prefix of ${Format.guild(guild)} to '${prefix}', because it wasn't cached.`);

        const inserted = await this.client.database.setPrefix(guild, prefix);
        cachedGuild.prefix = inserted.prefix;
    }

    syncPrefixes() {
        const { guilds } = this.client;
        for (const guild of guilds.values()) {
            const cachedGuild = this.get(guild.id);
            if (!cachedGuild)
                throw new Error(`Syncing Prefixes, ${Format.guild(guild)} was not cached.`);
            guild.commandPrefix = cachedGuild.prefix;
        }
    }
}

module.exports = GuildRegistry;