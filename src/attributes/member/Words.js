'use strict';

const MemberAttribute = require('../MemberAttribute.js');
const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const MathUtil = require('../../modules/MathUtil.js');
const StringUtil = require('../../modules/StringUtil.js');

class WordsAttribute extends MemberAttribute {
    constructor() {
        super({
            id: 'words',
            description: 'total words sent'
        });
    }

    async getCommand({ client }, member) {
        const { word_count, rank } = await client.master.database.guildMembers.getRankedWordsFor(member);

        return DiscordEmbedFormatter
            .baseUserEmbed(member.user)
            .setDescription([
                `${member.displayName} has sent ${StringUtil.plural(word_count, 'word', '**')} in this server.`,
                `They are the **${MathUtil.formatNumberSuffix(rank)}** user of the server (excluding users who left).`
            ].join('\n'));
    }

    rank(database, guild, entries) {
        return database.guildMembers.getRankedWords(guild, entries);
    }

    presentRankEntry(member, { rank, word_count }) {
        return `${rank}: ${member.user.username} - ${StringUtil.plural(word_count, 'word', '`')}`;
    }
}

module.exports = WordsAttribute;