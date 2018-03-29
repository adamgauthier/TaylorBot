'use strict';

const { GlobalPaths } = require('globalobjects');

const MessageWatcher = require(GlobalPaths.MessageWatcher);

class LastSpokeWatcher extends MessageWatcher {
    constructor() {
        super();
    }

    async messageHandler({ database }, message) {
        const { channel } = message;
        if (channel.type === 'text') {
            const { author, guild } = message;

            await database.updateLastSpoke(author, guild, message.createdAt.getTime());
        }
    }
}

module.exports = LastSpokeWatcher;