'use strict';

const { GlobalPaths } = require('globalobjects');

const ArgumentType = require(GlobalPaths.ArgumentType);

const ChannelArgumentType = require('./ChannelArgumentType');

class ChannelOrCurrentArgumentType extends ArgumentType {
    constructor() {
        super('channel-or-current');
        // TODO: Get it from registry?
        this.channelArgumentType = new ChannelArgumentType();
    }

    isEmpty() {
        return false;
    }

    parse(val, msg, arg) {
        if (!val) {
            return msg.channel;
        }

        return this.channelArgumentType.parse(val, msg, arg);
    }
}

module.exports = ChannelOrCurrentArgumentType;
