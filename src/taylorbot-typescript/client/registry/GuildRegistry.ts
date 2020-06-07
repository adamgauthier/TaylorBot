import Format = require('../../modules/DiscordFormatter.js');
import { RedisDriver } from '../../caching/RedisDriver';
import { DatabaseDriver } from '../../database/DatabaseDriver';
import { GuildMember, Guild } from 'discord.js';

export class GuildRegistry extends Map {
    #database: DatabaseDriver;
    #redis: RedisDriver;

    constructor(database: DatabaseDriver, redis: RedisDriver) {
        super();
        this.#database = database;
        this.#redis = redis;
    }

    async load(): Promise<void> {
        const guilds: { guild_id: string }[] = await this.#database.guilds.getAll();
        guilds.forEach(g => this.cacheGuild(g));
    }

    cacheGuild(databaseGuild: { guild_id: string }): void {
        this.set(databaseGuild.guild_id, {
            roleGroups: {}
        });
    }

    prefixKey(guild: Guild): string {
        return `prefix:guild:${guild.id}`;
    }

    async changePrefix(guild: Guild, prefix: string): Promise<void> {
        const cachedGuild = this.get(guild.id);

        if (!cachedGuild)
            throw new Error(`Could not change prefix of ${Format.guild(guild)} to '${prefix}', because it wasn't cached.`);

        const updated = await this.#database.guilds.setPrefix(guild, prefix);
        await this.#redis.set(this.prefixKey(guild), updated.prefix);
    }

    async getPrefix(guild: Guild): Promise<string> {
        const key = this.prefixKey(guild);
        const cachedPrefix = await this.#redis.get(key);

        if (!cachedPrefix) {
            const { prefix } = await this.#database.guilds.getPrefix(guild);
            await this.#redis.set(key, prefix);
            return prefix;
        }

        return cachedPrefix;
    }

    memberKey(member: GuildMember): string {
        return `command-user:guild:${member.guild.id}:user:${member.id}`;
    }

    async addOrUpdateMemberAsync(member: GuildMember, lastSpokeAt: Date): Promise<boolean> {
        const key = this.memberKey(member);
        const cachedMember = await this.#redis.get(key);

        if (!cachedMember) {
            const memberAdded = await this.#database.guildMembers.addOrUpdateMemberAsync(member, lastSpokeAt);
            await this.#redis.setExpire(
                key,
                1 * 60 * 60,
                '1'
            );
            return memberAdded;
        }

        return false;
    }
}
