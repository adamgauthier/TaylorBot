'use strict';

class ChannelCommandRegistry {
    constructor(database, redis) {
        this.database = database;
        this.redis = redis;
    }

    key(guildId, channelId) {
        return `enabled-commands:guild:${guildId}:channel:${channelId}`;
    }

    async isCommandDisabledInChannel(guildTextChannel, command) {
        const key = this.key(guildTextChannel.guild.id, guildTextChannel.id);
        const isEnabled = await this.redis.hashGet(key, command.name);

        if (isEnabled === null) {
            const { exists } = await this.database.channelCommands.getIsCommandDisabledInChannel(guildTextChannel, command);
            await this.redis.hashSet(key, command.name, (!exists) ? 1 : 0);
            await this.redis.expire(key, 6 * 60 * 60);
            return exists;
        }

        return isEnabled === '0';
    }

    async disableCommandInChannel(guildTextChannel, command) {
        await this.database.channelCommands.disableCommandInChannel(guildTextChannel, command);
        await this.redis.hashSet(
            this.key(guildTextChannel.guild.id, guildTextChannel.id),
            command.name,
            0
        );
    }

    async enableCommandInChannel(guildTextChannel, command) {
        await this.database.channelCommands.enableCommandInChannel(guildTextChannel, command);
        await this.redis.hashSet(
            this.key(guildTextChannel.guild.id, guildTextChannel.id),
            command.name,
            1
        );
    }
}

module.exports = ChannelCommandRegistry;
