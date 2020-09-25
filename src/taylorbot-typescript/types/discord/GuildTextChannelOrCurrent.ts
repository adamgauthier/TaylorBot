import { DMChannel, TextChannel } from 'discord.js';
import { CommandMessageContext } from '../../commands/CommandMessageContext';
import { MessageContext } from '../../structures/MessageContext';
import GuildTextChannelArgumentType = require('./GuildTextChannel.js');

class GuildTextChannelOrCurrentArgumentType extends GuildTextChannelArgumentType {
    get id(): string {
        return 'guild-text-channel-or-current';
    }

    canBeEmpty({ message }: MessageContext): boolean {
        return message.channel.type === 'text';
    }

    default({ message }: CommandMessageContext): TextChannel | DMChannel {
        return message.channel;
    }
}

export = GuildTextChannelOrCurrentArgumentType;
