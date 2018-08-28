'use strict';

const GuildTextChannelArgumentType = require('./GuildTextChannel.js');

class GuildTextChannelOrCurrentArgumentType extends GuildTextChannelArgumentType {
    get id() {
        return 'guild-text-channel-or-current';
    }

    canBeEmpty({ message }) {
        return message.channel.type === 'text';
    }

    default({ message }) {
        return message.channel;
    }
}

module.exports = GuildTextChannelOrCurrentArgumentType;
