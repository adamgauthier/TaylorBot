import { RedisDriver } from '../../caching/RedisDriver';
import { DatabaseDriver } from '../../database/DatabaseDriver';
import { GuildMember, Guild, TextChannel, NewsChannel } from 'discord.js';

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

    spamChannelKey(channel: TextChannel | NewsChannel): string {
        return `spam-channel:guild:${channel.guild.id}:channel:${channel.id}`;
    }

    async insertOrGetIsSpamChannelAsync(guildChannel: TextChannel | NewsChannel): Promise<boolean> {
        const key = this.spamChannelKey(guildChannel);
        const cachedSpamChannel = await this.#redis.get(key);

        if (cachedSpamChannel == null) {
            const isSpam = await this.#database.textChannels.insertOrGetIsSpamChannelAsync(guildChannel);
            await this.#redis.setExpire(
                key,
                1 * 60 * 60,
                isSpam ? '1' : '0'
            );
            return isSpam;
        }

        return Number.parseInt(cachedSpamChannel) !== 0;
    }

    async setSpamChannelAsync(guildChannel: TextChannel | NewsChannel): Promise<void> {
        const key = this.spamChannelKey(guildChannel);

        await this.#database.textChannels.upsertSpamChannel(guildChannel, true);
        await this.#redis.setExpire(
            key,
            1 * 60 * 60,
            '1'
        );
    }

    async removeSpamChannelAsync(guildChannel: TextChannel | NewsChannel): Promise<void> {
        const key = this.spamChannelKey(guildChannel);

        await this.#database.textChannels.upsertSpamChannel(guildChannel, false);
        await this.#redis.setExpire(
            key,
            1 * 60 * 60,
            '0'
        );
    }
}
