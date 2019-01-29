'use strict';

const MessageWatcher = require('../structures/MessageWatcher.js');
const StringUtil = require('../modules/StringUtil.js');

class ChannelMessageCountWatcher extends MessageWatcher {
    async messageHandler(client, message) {
        const { database } = client.master;
        const { channel } = message;
        if (channel.type === 'text') {
            const messageCount = 1;
            const textChannel = await database.textChannels.addMessages(channel, messageCount);

            if (!textChannel.is_spam) {
                const { member } = message;
                if (member) {
                    const wordCount = StringUtil.countWords(message.content);

                    database.guildMembers.addMessagesAndWords(member, messageCount, wordCount);
                }
            }
        }
    }
}

module.exports = ChannelMessageCountWatcher;