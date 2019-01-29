'use strict';

const MessageWatcher = require('../structures/MessageWatcher.js');

class LastSpokeWatcher extends MessageWatcher {
    async messageHandler({ master }, message) {
        const { channel } = message;
        if (channel.type === 'text') {
            const { member } = message;

            if (member) {
                await master.database.guildMembers.updateLastSpoke(member, message.createdTimestamp);
            }
        }
    }
}

module.exports = LastSpokeWatcher;