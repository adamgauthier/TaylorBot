import { GuildMember } from 'discord.js';
import { CommandMessageContext } from '../../commands/CommandMessageContext';
import { MessageContext } from '../../structures/MessageContext';
import MemberArgumentType = require('./Member.js');

class MemberOrAuthorArgumentType extends MemberArgumentType {
    get id(): string {
        return 'member-or-author';
    }

    canBeEmpty({ message }: MessageContext): boolean {
        return message.member ? true : false;
    }

    default({ message }: CommandMessageContext): GuildMember | null {
        return message.member;
    }
}

export = MemberOrAuthorArgumentType;
