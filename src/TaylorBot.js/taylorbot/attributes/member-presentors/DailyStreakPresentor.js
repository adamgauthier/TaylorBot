'use strict';

const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const MathUtil = require('../../modules/MathUtil.js');
const StringUtil = require('../../modules/StringUtil.js');

class DailyStreakPresentor {
    constructor(attribute) {
        this.attribute = attribute;
    }

    present(commandContext, member, { [this.attribute.columnName]: streak, rank }) {
        return DiscordEmbedFormatter
            .baseUserEmbed(member.user)
            .setDescription([
                `${member.displayName} is currently on a **${StringUtil.formatNumberString(streak)}** day payout streak.`,
                `They are the **${MathUtil.formatNumberSuffix(rank)}** user of the server (excluding users who left).`
            ].join('\n'));
    }

    presentRankEntry(member, { [this.attribute.columnName]: stat, rank }) {
        return `${rank}: ${member.user.username} - ${StringUtil.plural(stat, 'day', '`', true)}`;
    }
}

module.exports = DailyStreakPresentor;
