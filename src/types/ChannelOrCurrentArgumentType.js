'use strict';

const { ArgumentType } = require('discord.js-commando');

const ChannelArgumentType = require('./ChannelArgumentType');

class ChannelOrCurrentArgumentType extends ArgumentType {
    constructor(client) {
        super(client, 'channel-or-current');
        this.channelArgumentType = new ChannelArgumentType(client);
    }

    isEmpty() {
        return false;
    }

    validate() {
        return true;
    }

    parse(val, msg, arg) {
        if (!val) {
            return msg.channel;
        }

        const parsed = this.channelArgumentType.parse(val, msg, arg);

        return parsed || msg.channel;
    }
}

module.exports = ChannelOrCurrentArgumentType;
