'use strict';

const TextArgumentType = require('./Text.js');
const ArgumentParsingError = require('../structures/ArgumentParsingError.js');
const Log = require('../tools/Logger.js');
const Format = require('../modules/DiscordFormatter.js');

class MemberArgumentType extends TextArgumentType {
    get id() {
        return 'member';
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
            }

            const search = val.toLowerCase();
            const inexactMembers = guild.members.filter(MemberArgumentType.memberFilterInexact(search));
            if (inexactMembers.size === 0) {
                throw new ArgumentParsingError(`Could not find member '${val}'.`);
            }
            else if (inexactMembers.size === 1) {
                return inexactMembers.first();
            }

            const exactMembers = inexactMembers.filter(MemberArgumentType.memberFilterExact(search));

            return exactMembers.size > 0 ? exactMembers.first() : inexactMembers.first();
        }
        else {
            throw new ArgumentParsingError(`Can't find member '${val}' outside of a server.`);
        }
    }

    static memberFilterExact(search) {
        return member =>
            member.user.username.toLowerCase() === search ||
            (member.nickname && member.nickname.toLowerCase() === search) ||
            `${member.user.username.toLowerCase()}#${member.user.discriminator}` === search;
    }

    static memberFilterInexact(search) {
        return member =>
            member.user.username.toLowerCase().includes(search) ||
            (member.nickname && member.nickname.toLowerCase().includes(search)) ||
            `${member.user.username.toLowerCase()}#${member.user.discriminator}`.includes(search);
    }
}

module.exports = MemberArgumentType;
