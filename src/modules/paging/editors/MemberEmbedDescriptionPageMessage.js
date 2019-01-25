'use strict';

const EmbedDescriptionPageEditor = require('./EmbedDescriptionPageEditor.js');
const DiscordUtil = require('../../discord/DiscordUtil.js');

class MemberEmbedDescriptionPageMessage extends EmbedDescriptionPageEditor {
    constructor(embed, guild, formatLine) {
        super(embed);
        this.guild = guild;
        this.formatLine = formatLine;
    }

    async formatDescription(currentPage) {
        const descriptionLines = [];
        for (const memberLine of currentPage) {
            const member = await DiscordUtil.getMember(this.guild, memberLine.user_id);
            if (member) {
                descriptionLines.push(
                    this.formatLine(member, memberLine)
                );
            }
        }
        return descriptionLines.join('\n');
    }
}

module.exports = MemberEmbedDescriptionPageMessage;