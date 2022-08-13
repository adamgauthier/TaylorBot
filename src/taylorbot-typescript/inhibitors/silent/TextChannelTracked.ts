import { SilentInhibitor } from '../SilentInhibitor.js';
import { MessageContext } from '../../structures/MessageContext.js';
import { ChannelType } from 'discord.js';

class TextChannelTracked extends SilentInhibitor {
    async shouldBeBlocked({ message, client }: MessageContext): Promise<string | null> {
        const { channel } = message;

        if (channel.type === ChannelType.GuildText || channel.type === ChannelType.GuildNews || channel.isThread()) {
            await client.master.registry.guilds.insertOrGetIsSpamChannelAsync(channel);
        }

        return null;
    }
}

export = TextChannelTracked;
