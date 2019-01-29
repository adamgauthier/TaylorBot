'use strict';

const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const StringUtil = require('../../modules/StringUtil.js');
const MathUtil = require('../../modules/MathUtil.js');

class SimpleStatPresentor {
    constructor(attribute) {
        this.attribute = attribute;
    }

    present(commandContext, member, { [this.attribute.columnName]: stat, rank }) {
        return DiscordEmbedFormatter
            .baseUserEmbed(member.user)
            .setDescription([
                `${member.displayName} has ${StringUtil.plural(stat, this.attribute.singularName, '**')}.`,
                `They are the **${MathUtil.formatNumberSuffix(rank)}** user of the server (excluding users who left).`
            ].join('\n'));
    }

    presentRankEntry(member, { [this.attribute.columnName]: stat, rank }) {
        return `${rank}: ${member.user.username} - ${StringUtil.plural(stat, this.attribute.singularName, '`')}`;
    }
}

module.exports = SimpleStatPresentor;