'use strict';

const EmbedDescriptionPageEditor = require('./EmbedDescriptionPageEditor.js');
const DiscordUtil = require('../../discord/DiscordUtil.js');

class MemberEmbedDescriptionPageMessage extends EmbedDescriptionPageEditor {
    constructor(embed, database, guild, formatLine) {
        super(embed);
        this.database = database;
        this.guild = guild;
        this.formatLine = formatLine;
    }

    async formatDescription(currentPage) {
        const deadMembers = [];
        const descriptionLines = [];

        for (const memberLine of currentPage) {
            const member = await DiscordUtil.getMember(this.guild, memberLine.user_id);
            if (member) {
                descriptionLines.push(
                    this.formatLine(member, memberLine)
                );
            }
            else {
                deadMembers.push(memberLine.user_id);
            }
        }

        if (deadMembers.length > 0)
            await this.database.guildMembers.setDead(deadMembers);

        return descriptionLines.join('\n');
    }
}

module.exports = MemberEmbedDescriptionPageMessage;
