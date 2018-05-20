'use strict';

const { Paths } = require('globalobjects');

const ArgumentType = require(Paths.ArgumentType);
const ArgumentParsingError = require(Paths.ArgumentParsingError);
const Log = require(Paths.Logger);
const Format = require(Paths.DiscordFormatter);

class MemberArgumentType extends ArgumentType {
    constructor() {
        super('member');
    }

    async parse(val, message) {
        const { guild } = message;

        if (guild) {
            const matches = val.match(/^(?:<@!?)?([0-9]+)>?$/);
            if (matches) {
                try {
                    const member = await guild.members.fetch(matches[1]);

                    if (member) {
                        return member;
                    }
                }
                catch (e) {
                    Log.error(`Error occurred while fetching member '${matches[1]}' for guild ${Format.guild(guild)} in MemberArgumentType parsing: ${e}`);
                }
            }

            const search = val.toLowerCase();
            const inexactMembers = guild.members.filterArray(MemberArgumentType.memberFilterInexact(search));
            if (inexactMembers.length === 0) {
                throw new ArgumentParsingError(`Could not find member '${val}'.`);
            }
            else if (inexactMembers.length === 1) {
                return inexactMembers[0];
            }

            const exactMembers = inexactMembers.filter(MemberArgumentType.memberFilterExact(search));

            return exactMembers.length > 0 ? exactMembers[0] : inexactMembers[0];
        }

        throw new ArgumentParsingError(`Can't find member '${val}' outside of a server.`);
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
