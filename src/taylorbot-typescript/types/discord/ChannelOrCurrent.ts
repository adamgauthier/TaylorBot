import { DMChannel, NewsChannel, PartialDMChannel, TextChannel, ThreadChannel, VoiceChannel } from 'discord.js';
import { CommandMessageContext } from '../../commands/CommandMessageContext';
import ChannelArgumentType = require('./Channel.js');

class ChannelOrCurrentArgumentType extends ChannelArgumentType {
    get id(): string {
        return 'channel-or-current';
    }

    canBeEmpty(): boolean {
        return true;
    }

    default({ message }: CommandMessageContext): ThreadChannel | DMChannel | PartialDMChannel | NewsChannel | TextChannel | VoiceChannel {
        return message.channel;
    }
}

export = ChannelOrCurrentArgumentType;
