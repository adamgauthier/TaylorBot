'use strict';

const { GlobalPaths } = require('globalobjects');

const MessageWatcher = require(GlobalPaths.MessageWatcher);

class LastSpokeWatcher extends MessageWatcher {
    async messageHandler({ database }, message) {
        const { channel } = message;
        if (channel.type === 'text') {
            const { member } = message;

            if (member) {
                await database.guildMembers.updateLastSpoke(member, message.createdTimestamp);
            }
        }
    }
}

module.exports = LastSpokeWatcher;