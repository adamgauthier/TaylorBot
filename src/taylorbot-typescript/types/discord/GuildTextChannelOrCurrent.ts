import { DMChannel, NewsChannel, TextChannel } from 'discord.js';
import { CommandMessageContext } from '../../commands/CommandMessageContext';
import { MessageContext } from '../../structures/MessageContext';
import GuildTextChannelArgumentType = require('./GuildTextChannel.js');

class GuildTextChannelOrCurrentArgumentType extends GuildTextChannelArgumentType {
    get id(): string {
        return 'guild-text-channel-or-current';
    }

    canBeEmpty(context: MessageContext): boolean {
        return context.isGuild;
    }

    default({ message }: CommandMessageContext): TextChannel | DMChannel | NewsChannel {
        return message.channel;
    }
}

export = GuildTextChannelOrCurrentArgumentType;
