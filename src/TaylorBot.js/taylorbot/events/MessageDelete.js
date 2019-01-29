'use strict';

const { Events } = require('discord.js').Constants;

const EventHandler = require('../structures/EventHandler.js');
const StringUtil = require('../modules/StringUtil.js');

class Message extends EventHandler {
    constructor() {
        super(Events.MESSAGE_DELETE);
    }

    async handler(client, message) {
        const { database } = client.master;
        const { channel } = message;
        if (channel.type === 'text') {
            const messageCount = 1;
            const textChannel = await database.textChannels.removeMessages(channel, messageCount);

            if (!textChannel.is_spam) {
                const { member } = message;
                if (member) {
                    const wordCount = StringUtil.countWords(message.content);

                    database.guildMembers.removeMessagesAndWords(member, messageCount, wordCount);
                }
            }
        }
    }
}

module.exports = Message;