'use strict';

const ArrayUtil = require('../../modules/ArrayUtil.js');

class ChannelCommandRegistry {
    constructor(database, redis) {
        this.database = database;
        this.redis = redis;
    }

    key(guildId, channelId) {
        return `${guildId}:${channelId}`;
    }

    async cacheChannelCommands() {
        const groupedChannelCommands = ArrayUtil.groupBy(
            await this.database.channelCommands.getAll(),
            channelCommand => this.key(channelCommand.guild_id, channelCommand.channel_id)
        );

        for (const [key, channelCommands] of groupedChannelCommands.entries()) {
            await this.redis.setAdd(key, channelCommands.map(c => c.command_id));
        }
    }

    async isCommandDisabledInChannel(guildTextChannel, command) {
        const result = await this.redis.setIsMember(
            this.key(guildTextChannel.guild.id, guildTextChannel.id),
            command.name
        );
        return !!result;
    }

    async disableCommandInChannel(guildTextChannel, command) {
        await this.database.channelCommands.disableCommandInChannel(guildTextChannel, command);
        await this.redis.setAdd(
            this.key(guildTextChannel.guild.id, guildTextChannel.id),
            command.name
        );
    }

    async enableCommandInChannel(guildTextChannel, command) {
        await this.database.channelCommands.enableCommandInChannel(guildTextChannel, command);
        await this.redis.setRemove(
            this.key(guildTextChannel.guild.id, guildTextChannel.id),
            command.name
        );
    }
}

module.exports = ChannelCommandRegistry;