import WordArgumentType = require('../base/Word');
import MentionedMemberArgumentType = require('./MentionedMember.js');
import { ArgumentParsingError } from '../ArgumentParsingError';
import { CommandArgumentInfo, CommandMessageContext } from '../../commands/CommandMessageContext';
import { User } from 'discord.js';

class MentionedUserArgumentType extends WordArgumentType {
    readonly #mentionedMemberArgumentType = new MentionedMemberArgumentType();

    get id(): string {
        return 'mentioned-user';
    }

    async parse(val: string, commandContext: CommandMessageContext, arg: CommandArgumentInfo): Promise<User> {
        const { message, client } = commandContext;
        if (message.guild) {
            const member = await this.#mentionedMemberArgumentType.parse(val, commandContext, arg);
            return member.user;
        }
        else {
            const matches = val.trim().match(/^(?:<@!?)?([0-9]+)>?$/);
            const sharedGuilds = client.guilds.filter(g => g.members.has(message.author!.id));

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

export = MentionedUserArgumentType;
