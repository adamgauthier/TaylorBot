'use strict';

const MemberAttribute = require('../MemberAttribute.js');
const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const ArrayEmbedDescriptionPageMessage = require('../../modules/paging/ArrayEmbedDescriptionPageMessage.js');
const MathUtil = require('../../modules/MathUtil.js');
const TimeUtil = require('../../modules/TimeUtil.js');
const ArrayUtil = require('../../modules/ArrayUtil.js');

class JoinedAttribute extends MemberAttribute {
    constructor() {
        super({
            id: 'joined',
            description: 'first joined date'
        });
    }

    async retrieve({ client }, member) {
        const { first_joined_at, rank } = await client.master.database.guildMembers.getRankedFirstJoinedAtFor(member);

        return DiscordEmbedFormatter
            .baseUserEmbed(member.user)
            .setDescription([
                `${member.displayName} first joined the server on **${TimeUtil.formatFull(first_joined_at)}**.`,
                `They were the **${MathUtil.formatNumberSuffix(rank)}** user to join.`
            ].join('\n'));
    }

    async rank({ message, client }, guild) {
        const members = await client.master.database.guildMembers.getRankedFirstJoinedAt(guild, 100);

        const embed = DiscordEmbedFormatter.baseGuildHeader(guild);
        const lines = members.map(m => `${m.rank}: <@${m.user_id}> - ${TimeUtil.formatMini(m.first_joined_at)}`);

        return new ArrayEmbedDescriptionPageMessage(
            client,
            message.author,
            embed,
            ArrayUtil.chunk(lines, 20).map(chunk => chunk.join('\n'))
        );
    }
}

module.exports = JoinedAttribute;