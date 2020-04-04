'use strict';

const TextArgumentType = require('../base/Text.js');
const ArgumentParsingError = require('../ArgumentParsingError.js');
const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

class MentionedMemberArgumentType extends TextArgumentType {
    get id() {
        return 'mentioned-member';
    }

    async parse(val, { message }) {
        const { guild } = message;

        if (guild) {
            const matches = val.trim().match(/^(?:<@!?)?([0-9]+)>?$/);
            if (matches) {
                const match = matches[1];

                const member = guild.members.resolve(match);
                if (member)
                    return member;

                try {
                    const fetchedMember = await guild.members.fetch(match);
                    return fetchedMember;
                }
                catch (e) {
                    Log.error(`Error occurred while fetching member '${match}' for guild ${Format.guild(guild)} in MemberArgumentType parsing: ${e}`);
                }

                throw new ArgumentParsingError(`Can't parse '${val}' into a mentioned member. Are you sure they are part of the server?`);
            }
            else {
                throw new ArgumentParsingError(`Can't parse '${val}' into a mentioned member. You must mention the user with @.`);
            }
        }
        else {
            throw new ArgumentParsingError(`Can't find mentioned member '${val}' outside of a server.`);
        }
    }
}

module.exports = MentionedMemberArgumentType;
