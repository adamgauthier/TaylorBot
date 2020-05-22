'use strict';

const TextArgumentType = require('../base/Text.js');
const MemberArgumentType = require('./Member.js');
const ArgumentParsingError = require('../ArgumentParsingError.js');

class UserArgumentType extends TextArgumentType {
    constructor() {
        super();
        this.memberArgumentType = new MemberArgumentType();
    }

    get id() {
        return 'user';
    }

    async parse(val, commandContext, info) {
        const member = await this._parse(val, commandContext, info);

        await commandContext.client.master.registry.users.getIgnoredUntil(member.user);
        await commandContext.client.master.registry.guilds.addOrUpdateMemberAsync(member, null);

        return member.user;
    }

    async _parse(val, commandContext, info) {
        const { message, client } = commandContext;
        if (message.guild) {
            const member = await this.memberArgumentType._parse(val, commandContext, info);
            return member;
        }
        else {
            const matches = val.trim().match(/^(?:<@!?)?([0-9]+)>?$/);
            const sharedGuilds = client.guilds.filter(g => g.members.has(message.author.id));

            for (const guild of sharedGuilds.values()) {
                if (matches) {
                    const member = guild.members.get(matches[1]);
                    if (member)
                        return member;
                }

                const search = val.toLowerCase();
                const inexactMembers = guild.members.filter(MemberArgumentType.memberFilterInexact(search));

                if (inexactMembers.size === 1) {
                    return inexactMembers.first();
                }
                else if (inexactMembers.size > 1) {
                    const exactMembers = inexactMembers.filter(MemberArgumentType.memberFilterExact(search));

                    return exactMembers.size > 0 ? exactMembers.first() : inexactMembers.first();
                }
            }

            throw new ArgumentParsingError(`Could not find user '${val}' in servers that we share. Use the command in the server directly.`);
        }
    }
}

module.exports = UserArgumentType;
