'use strict';

const { Paths } = require('globalobjects');

const ArgumentType = require(Paths.ArgumentType);

const ChannelArgumentType = require('./ChannelArgumentType');

class ChannelOrCurrentArgumentType extends ArgumentType {
    constructor() {
        super('channel-or-current');
        // TODO: Get it from registry?
        this.channelArgumentType = new ChannelArgumentType();
    }

    canBeEmpty() {
        return true;
    }

    parse(val, messageContext, arg) {
        if (!val) {
            return messageContext.message.channel;
        }

        return this.channelArgumentType.parse(val, messageContext, arg);
    }
}

module.exports = ChannelOrCurrentArgumentType;
