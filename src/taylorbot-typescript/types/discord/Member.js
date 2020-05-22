'use strict';

const TextArgumentType = require('../base/Text.js');
const ArgumentParsingError = require('../ArgumentParsingError.js');
const MentionedMemberArgumentType = require('./MentionedMember.js');

class MemberArgumentType extends TextArgumentType {
    constructor() {
        super();
        this.mentionedMemberArgumentType = new MentionedMemberArgumentType();
    }

    get id() {
        return 'member';
    }

    async parse(val, commandContext, info) {
        const member = await this._parse(val, commandContext, info);

        await commandContext.client.master.registry.users.getIgnoredUntil(member.user);
        await commandContext.client.master.registry.guilds.addOrUpdateMemberAsync(member, null);

        return member;
    }

    async _parse(val, commandContext, info) {
        const { guild } = commandContext.message;

        if (guild) {
            try {
                const member = await this.mentionedMemberArgumentType.parse(val, commandContext, info);
                return member;
            }
            catch (e) {
                // Empty
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
