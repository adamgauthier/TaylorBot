import { DatabaseDriver } from '../../database/DatabaseDriver';
import { RedisDriver } from '../../caching/RedisDriver';
import { BaseGuildTextChannel, ThreadChannel } from 'discord.js';
import { CachedCommand } from './CachedCommand';

export class ChannelCommandRegistry {
    readonly #database: DatabaseDriver;
    readonly #redis: RedisDriver;

    constructor(database: DatabaseDriver, redis: RedisDriver) {
        this.#database = database;
        this.#redis = redis;
    }

    key(guildId: string, channelId: string): string {
        return `enabled-commands:guild:${guildId}:channel:${channelId}`;
    }

    async isCommandDisabledInChannel(guildTextChannel: BaseGuildTextChannel | ThreadChannel, command: CachedCommand): Promise<boolean> {
        const key = this.key(guildTextChannel.guild.id, guildTextChannel.id);
        const isEnabled = await this.#redis.hashGet(key, command.name);

        if (isEnabled === null) {
            const { exists } = await this.#database.channelCommands.getIsCommandDisabledInChannel(guildTextChannel, command);
            await this.#redis.hashSet(key, command.name, (!exists) ? '1' : '0');
            await this.#redis.expire(key, 6 * 60 * 60);
            return exists;
        }

        return isEnabled === '0';
    }
}
