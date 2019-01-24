'use strict';

const Format = require('../../modules/DiscordFormatter.js');

class GuildRegistry extends Map {
    constructor(database) {
        super();
        this.database = database;
    }

    async load() {
        const guilds = await this.database.guilds.getAll();
        guilds.forEach(g => this.cacheGuild(g));
    }

    cacheGuild(databaseGuild) {
        this.set(databaseGuild.guild_id, {
            'prefix': databaseGuild.prefix,
            'roleGroups': {}
        });
    }

    async addGuild(guild) {
        if (this.has(guild.id)) {
            throw new Error(`Adding guild ${Format.guild(guild)}, already cached.`);
        }

        const databaseGuild = await this.database.guilds.add(guild);
        this.cacheGuild(databaseGuild);
    }

    async changePrefix(guild, prefix) {
        const cachedGuild = this.get(guild.id);

        if (!cachedGuild)
            throw new Error(`Could not change prefix of ${Format.guild(guild)} to '${prefix}', because it wasn't cached.`);

        if (cachedGuild.prefix === prefix)
            throw new Error(`Could not change prefix of ${Format.guild(guild)} to '${prefix}', because it already had that prefix.`);

        const updated = await this.database.guilds.setPrefix(guild, prefix);
        cachedGuild.prefix = updated.prefix;
    }
}

module.exports = GuildRegistry;