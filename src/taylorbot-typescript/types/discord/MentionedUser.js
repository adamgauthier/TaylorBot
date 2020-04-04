'use strict';

const WordArgumentType = require('../base/Word.js');
const MentionedMemberArgumentType = require('./MentionedMember.js');
const ArgumentParsingError = require('../ArgumentParsingError.js');

class MentionedUserArgumentType extends WordArgumentType {
    constructor() {
        super();
        this.mentionedMemberArgumentType = new MentionedMemberArgumentType();
    }

    get id() {
        return 'mentioned-user';
    }

    async parse(val, commandContext, info) {
        const { message, client } = commandContext;
        if (message.guild) {
            const member = await this.mentionedMemberArgumentType.parse(val, commandContext, info);
            return member.user;
        }
        else {
            const matches = val.trim().match(/^(?:<@!?)?([0-9]+)>?$/);
            const sharedGuilds = client.guilds.filter(g => g.members.has(message.author.id));

            for (const guild of sharedGuilds.values()) {
                if (matches) {
                    const member = guild.members.get(matches[1]);
                    if (member)
                        return member.user;
                }
            }

            throw new ArgumentParsingError(`Could not find mentioned user '${val}' in servers that we share. Use the command in the server directly.`);
        }
    }
}

module.exports = MentionedUserArgumentType;