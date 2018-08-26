'use strict';

const MemberAttribute = require('../MemberAttribute.js');
const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const MathUtil = require('../../modules/MathUtil.js');
const TimeUtil = require('../../modules/TimeUtil.js');

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
}

module.exports = JoinedAttribute;