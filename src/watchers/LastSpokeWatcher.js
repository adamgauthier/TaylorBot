'use strict';

const { GlobalPaths } = require('globalobjects');

const MessageWatcher = require(GlobalPaths.MessageWatcher);
const database = require(GlobalPaths.databaseDriver);

class LastSpokeWatcher extends MessageWatcher {
    constructor() {
        super(async message => {
            const { channel } = message;
            if (channel.type === 'text') {
                const { author, guild } = message;

                await database.updateLastSpoke(author, guild, message.createdAt.getTime());
            }
        });
    }
}

module.exports = new LastSpokeWatcher();