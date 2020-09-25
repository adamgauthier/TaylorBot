import { Guild } from 'discord.js';
import { CommandMessageContext } from '../../commands/CommandMessageContext';
import { MessageContext } from '../../structures/MessageContext';
import GuildArgumentType = require('./Guild.js');

class GuildOrCurrentArgumentType extends GuildArgumentType {
    get id(): string {
        return 'guild-or-current';
    }

    canBeEmpty({ message }: MessageContext): boolean {
        return message.guild ? true : false;
    }

    default({ message }: CommandMessageContext): Guild | null {
        return message.guild;
    }
}

export = GuildOrCurrentArgumentType;
