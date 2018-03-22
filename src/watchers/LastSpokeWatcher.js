'use strict';

const { GlobalPaths } = require('globalobjects');

const MessageWatcher = require(GlobalPaths.MessageWatcher);
const taylorbot = require(GlobalPaths.taylorBotClient);

class LastSpokeWatcher extends MessageWatcher {
    constructor() {
        super(async message => {
            const { channel } = message;
            if (channel.type === 'text') {
                const { author, guild } = message;

                await taylorbot.database.updateLastSpoke(author, guild, message.createdAt.getTime());
            }
        });
    }
}

module.exports = new LastSpokeWatcher();