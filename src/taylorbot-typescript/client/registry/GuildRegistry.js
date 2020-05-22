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

    memberKey(member) {
        return `command-user:guild:${member.guild.id}:user:${member.id}`;
    }

    async addOrUpdateMemberAsync(member, lastSpokeAt) {
        const key = this.memberKey(member);
        const cachedMember = await this.redis.get(key);

        if (!cachedMember) {
            const memberAdded = await this.database.guildMembers.addOrUpdateMemberAsync(member, lastSpokeAt);
            await this.redis.setExpire(
                key,
                1 * 60 * 60,
                1
            );
            return memberAdded;
        }

        return false;
    }
}

module.exports = GuildRegistry;
