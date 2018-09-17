'use strict';

const MemberAttribute = require('../MemberAttribute.js');
const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const ArrayEmbedMemberDescriptionPageMessage = require('../../modules/paging/ArrayEmbedMemberDescriptionPageMessage.js');
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

        const embed = DiscordEmbedFormatter
            .baseGuildHeader(guild)
            .setTitle(`Ranking of ${this.description}`);

        return new ArrayEmbedMemberDescriptionPageMessage(
            client,
            message.author,
            ArrayUtil.chunk(members, 20),
            embed,
            guild,
            (member, { rank, first_joined_at }) => `${rank}: ${member.user.username} - ${TimeUtil.formatMini(first_joined_at)}`
        );
    }
}

module.exports = JoinedAttribute;