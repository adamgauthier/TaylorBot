'use strict';

const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const TimeUtil = require('../../modules/TimeUtil.js');
const MathUtil = require('../../modules/MathUtil.js');

class JoinedPresentor {
    constructor(attribute) {
        this.attribute = attribute;
    }

    present(commandContext, member, { [this.attribute.columnName]: firstJoinedAt, rank }) {
        return DiscordEmbedFormatter
            .baseUserEmbed(member.user)
            .setDescription([
                `${member.displayName} first joined the server on **${TimeUtil.formatFull(firstJoinedAt)}**.`,
                `They were the **${MathUtil.formatNumberSuffix(rank)}** user to join.`
            ].join('\n'));
    }

    presentRankEntry(member, { [this.attribute.columnName]: firstJoinedAt, rank }) {
        return `${rank}: ${member.user.username} - ${TimeUtil.formatMini(firstJoinedAt)}`;
    }
}

module.exports = JoinedPresentor;