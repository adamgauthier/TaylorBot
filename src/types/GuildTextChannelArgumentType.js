'use strict';

const { Paths } = require('globalobjects');

const ArgumentType = require(Paths.ArgumentType);
const ArgumentParsingError = require('../structures/ArgumentParsingError.js');
const ChannelArgumentType = require('./ChannelArgumentType.js');

class GuildTextChannelArgumentType extends ArgumentType {
    constructor() {
        super();
        this.channelArgumentType = new ChannelArgumentType();
    }

    get id() {
        return 'guild-text-channel';
    }

    parse(val, commandContext, info) {
        const channel = this.channelArgumentType.parse(val, commandContext, info);

        if (channel.type !== 'text')
            throw new ArgumentParsingError(`Channel ${channel.name} (\`${channel.id}\`) is not a text channel.`);

        return channel;
    }
}

module.exports = GuildTextChannelArgumentType;
