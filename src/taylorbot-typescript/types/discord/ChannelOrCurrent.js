'use strict';

const ChannelArgumentType = require('./Channel.js');

class ChannelOrCurrentArgumentType extends ChannelArgumentType {
    get id() {
        return 'channel-or-current';
    }

    canBeEmpty() {
        return true;
    }

    default({ message }) {
        return message.channel;
    }
}

module.exports = ChannelOrCurrentArgumentType;
