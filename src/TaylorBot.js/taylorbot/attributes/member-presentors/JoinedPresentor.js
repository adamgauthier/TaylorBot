'use strict';

const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const TimeUtil = require('../../modules/TimeUtil.js');
const MathUtil = require('../../modules/MathUtil.js');
const DiscordFormatter = require('../../modules/DiscordFormatter.js');

class JoinedPresentor {
    constructor(attribute) {
        this.attribute = attribute;
    }

    present(commandContext, member, { [this.attribute.columnName]: firstJoinedAt, rank } = {}) {
        return DiscordEmbedFormatter
            .baseUserEmbed(member.user)
            .setDescription(firstJoinedAt ? [
                `${member.displayName} first joined the server on **${TimeUtil.formatFull(firstJoinedAt.getTime())}**.`,
                `They were the **${MathUtil.formatNumberSuffix(rank)}** user to join.`
            ].join('\n') : `I don't know when ${member.displayName} first joined the server.`);
    }

    presentRankEntry(member, { [this.attribute.columnName]: firstJoinedAt, rank }) {
        return `${rank}: ${DiscordFormatter.escapeDiscordMarkdown(member.user.username)} - ${TimeUtil.formatMini(firstJoinedAt.getTime())}`;
    }
}

module.exports = JoinedPresentor;
