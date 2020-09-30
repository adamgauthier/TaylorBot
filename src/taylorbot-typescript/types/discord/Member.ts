import TextArgumentType = require('../base/Text');
import { ArgumentParsingError } from '../ArgumentParsingError';
import MentionedMemberArgumentType = require('./MentionedMember.js');
import { CommandArgumentInfo, CommandMessageContext } from '../../commands/CommandMessageContext';
import { GuildMember } from 'discord.js';

class MemberArgumentType extends TextArgumentType {
    readonly #mentionedMemberArgumentType = new MentionedMemberArgumentType();

    get id(): string {
        return 'member';
    }

    async parse(val: string, commandContext: CommandMessageContext, arg: CommandArgumentInfo): Promise<GuildMember> {
        const member = await this._parse(val, commandContext, arg);

        await commandContext.client.master.registry.users.getIgnoredUntil(member.user);
        await commandContext.client.master.registry.guilds.addOrUpdateMemberAsync(member, null);

        return member;
    }

    async _parse(val: string, commandContext: CommandMessageContext, arg: CommandArgumentInfo): Promise<GuildMember> {
        const { guild } = commandContext.message;

        if (guild) {
            try {
                const member = await this.#mentionedMemberArgumentType.parse(val, commandContext, arg);
                return member;
            }
            catch (e) {
                // Empty
            }

            const search = val.toLowerCase();
            const inexactMembers = guild.members.cache.filter(MemberArgumentType.memberFilterInexact(search));
            if (inexactMembers.size === 0) {
                throw new ArgumentParsingError(`Could not find member '${val}'.`);
            }
            else if (inexactMembers.size === 1) {
                return inexactMembers.first()!;
            }

            const exactMembers = inexactMembers.filter(MemberArgumentType.memberFilterExact(search));

            return exactMembers.size > 0 ? exactMembers.first()! : inexactMembers.first()!;
        }
        else {
            throw new ArgumentParsingError(`Can't find member '${val}' outside of a server.`);
        }
    }

    static memberFilterExact(search: string) {
        return (member: GuildMember): boolean =>
            member.user.username.toLowerCase() === search ||
            (member.nickname && member.nickname.toLowerCase() === search) ||
            `${member.user.username.toLowerCase()}#${member.user.discriminator}` === search;
    }

    static memberFilterInexact(search: string) {
        return (member: GuildMember): boolean =>
            member.user.username.toLowerCase().includes(search) ||
            (member.nickname && member.nickname.toLowerCase().includes(search)) ||
            `${member.user.username.toLowerCase()}#${member.user.discriminator}`.includes(search);
    }
}

export = MemberArgumentType;
