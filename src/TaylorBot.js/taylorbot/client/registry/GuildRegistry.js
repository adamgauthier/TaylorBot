'use strict';

const Format = require('../../modules/DiscordFormatter.js');

class GuildRegistry extends Map {
    constructor(database, redis) {
        super();
        this.database = database;
        this.redis = redis;
    }

    async load() {
        const guilds = await this.database.guilds.getAll();
        guilds.forEach(g => this.cacheGuild(g));
    }

    cacheGuild(databaseGuild) {
        this.set(databaseGuild.guild_id, {
            roleGroups: {}
        });
    }

    prefixKey(guild) {
        return `prefix:guild:${guild.id}`;
    }

    async changePrefix(guild, prefix) {
        const cachedGuild = this.get(guild.id);

        if (!cachedGuild)
            throw new Error(`Could not change prefix of ${Format.guild(guild)} to '${prefix}', because it wasn't cached.`);

        if (cachedGuild.prefix === prefix)
            throw new Error(`Could not change prefix of ${Format.guild(guild)} to '${prefix}', because it already had that prefix.`);

        const updated = await this.database.guilds.setPrefix(guild, prefix);
        await this.redis.set(this.prefixKey(guild), updated.prefix);
    }

    async getPrefix(guild) {
        const key = this.prefixKey(guild);
        const cachedPrefix = await this.redis.get(key);

        if (!cachedPrefix) {
            const { prefix } = await this.database.guilds.getPrefix(guild);
            await this.redis.set(key, prefix);
            return prefix;
        }

        return cachedPrefix;
    }
}

module.exports = GuildRegistry;
