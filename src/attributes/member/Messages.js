'use strict';

const MemberAttribute = require('../MemberAttribute.js');
const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const MathUtil = require('../../modules/MathUtil.js');
const StringUtil = require('../../modules/StringUtil.js');

class MessagesAttribute extends MemberAttribute {
    constructor() {
        super({
            id: 'messages',
            description: 'total messages sent'
        });
    }

    async getCommand({ client }, member) {
        const { message_count, rank } = await client.master.database.guildMembers.getRankedMessagesFor(member);

        return DiscordEmbedFormatter
            .baseUserEmbed(member.user)
            .setDescription([
                `${member.displayName} has sent ${StringUtil.plural(message_count, 'message', '**')} in this server.`,
                `They are the **${MathUtil.formatNumberSuffix(rank)}** user of the server (excluding users who left).`
            ].join('\n'));
    }

    rank(database, guild, entries) {
        return database.guildMembers.getRankedMessages(guild, entries);
    }

    presentRankEntry(member, { rank, message_count }) {
        return `${rank}: ${member.user.username} - ${StringUtil.plural(message_count, 'message', '`')}`;
    }
}

module.exports = MessagesAttribute;