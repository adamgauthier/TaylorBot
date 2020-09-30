import { DMChannel, NewsChannel, TextChannel } from 'discord.js';
import { CommandMessageContext } from '../../commands/CommandMessageContext';
import ChannelArgumentType = require('./Channel.js');

class ChannelOrCurrentArgumentType extends ChannelArgumentType {
    get id(): string {
        return 'channel-or-current';
    }

    canBeEmpty(): boolean {
        return true;
    }

    default({ message }: CommandMessageContext): TextChannel | DMChannel | NewsChannel {
        return message.channel;
    }
}

export = ChannelOrCurrentArgumentType;
