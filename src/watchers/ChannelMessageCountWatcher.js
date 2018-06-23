'use strict';

const { Paths } = require('globalobjects');

const MessageWatcher = require(Paths.MessageWatcher);

class LastSpokeWatcher extends MessageWatcher {
    async messageHandler({ master }, message) {
        const { channel } = message;
        if (channel.type === 'text') {
            await master.database.textChannels.addMessagesCount(channel, 1);
        }
    }
}

module.exports = LastSpokeWatcher;