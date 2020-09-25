import TextArgumentType = require('../base/Text');
import MemberArgumentType = require('./Member.js');
import { ArgumentParsingError } from '../ArgumentParsingError';
import { CommandArgumentInfo, CommandMessageContext } from '../../commands/CommandMessageContext';
import { GuildMember, User } from 'discord.js';

class UserArgumentType extends TextArgumentType {
    readonly #memberArgumentType = new MemberArgumentType();

    get id(): string {
        return 'user';
    }

    async parse(val: string, commandContext: CommandMessageContext, arg: CommandArgumentInfo): Promise<User> {
        const member = await this._parse(val, commandContext, arg);

        await commandContext.client.master.registry.users.getIgnoredUntil(member.user);
        await commandContext.client.master.registry.guilds.addOrUpdateMemberAsync(member, null);

        return member.user;
    }

    async _parse(val: string, commandContext: CommandMessageContext, arg: CommandArgumentInfo): Promise<GuildMember> {
        const { message, client } = commandContext;
        if (message.guild) {
            const member = await this.#memberArgumentType._parse(val, commandContext, arg);
            return member;
        }
        else {
            const matches = val.trim().match(/^(?:<@!?)?([0-9]+)>?$/);
            const sharedGuilds = client.guilds.filter(g => g.members.has(message.author!.id));

            for (const guild of sharedGuilds.values()) {
                if (matches) {
                    const member = guild.members.get(matches[1]);
                    if (member)
                        return member;
                }

                const search = val.toLowerCase();
                const inexactMembers = guild.members.filter(MemberArgumentType.memberFilterInexact(search));

                if (inexactMembers.size === 1) {
                    return inexactMembers.first()!;
                }
                else if (inexactMembers.size > 1) {
                    const exactMembers = inexactMembers.filter(MemberArgumentType.memberFilterExact(search));

                    return exactMembers.size > 0 ? exactMembers.first()! : inexactMembers.first()!;
                }
            }

            throw new ArgumentParsingError(`Could not find user '${val}' in servers that we share. Use the command in the server directly.`);
        }
    }
}

export = UserArgumentType;
