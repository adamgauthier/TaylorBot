'use strict';

const WordArgumentType = require('../base/Word.js');
const ChannelArgumentType = require('./Channel.js');

class GuildTextChannelArgumentType extends WordArgumentType {
    constructor() {
        super();
        this.channelArgumentType = new ChannelArgumentType({
            channelFilter: channel => channel.type === 'text'
        });
    }

    get id() {
        return 'guild-text-channel';
    }

    parse(val, commandContext, info) {
        return this.channelArgumentType.parse(val, commandContext, info);
    }
}

module.exports = GuildTextChannelArgumentType;
