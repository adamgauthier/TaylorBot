'use strict';

const ArrayEmbedDescriptionPageMessage = require('./ArrayEmbedDescriptionPageMessage.js');

class ArrayEmbedMemberDescriptionPageMessage extends ArrayEmbedDescriptionPageMessage {
    constructor(client, owner, pages, embed, guild, formatLine) {
        super(client, owner, pages, embed);
        this.guild = guild;
        this.formatLine = formatLine;
    }

    async formatDescription(currentPage) {
        const descriptionLines = [];
        for (const memberLine of currentPage) {
            const member = await this.getMember(memberLine.user_id);
            if (member) {
                descriptionLines.push(
                    this.formatLine(member, memberLine)
                );
            }
        }
        return descriptionLines.join('\n');
    }

    async getMember(userId) {
        if (this.guild.members.has(userId)) {
            return this.guild.members.get(userId);
        }
        else {
            try {
                const fetchedMember = await this.guild.members.fetch(userId);
                return fetchedMember;
            } catch (e) {
                //Not a member
            }
        }

        return null;
    }
}

module.exports = ArrayEmbedMemberDescriptionPageMessage;