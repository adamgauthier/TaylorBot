import { RedisDriver } from '../../caching/RedisDriver';
import { DatabaseDriver } from '../../database/DatabaseDriver';
import { GuildMember, Guild } from 'discord.js';

export class GuildRegistry extends Map<string, { roleGroups: Record<string, string | undefined> }> {
    readonly #database: DatabaseDriver;
    readonly #redis: RedisDriver;

    constructor(database: DatabaseDriver, redis: RedisDriver) {
        super();
        this.#database = database;
        this.#redis = redis;
    }

    async load(): Promise<void> {
        const guilds = await this.#database.guilds.getAll();
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

    async addOrUpdateMemberAsync(member: GuildMember, lastSpokeAt: Date | null): Promise<boolean> {
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
