import { SilentInhibitor } from '../SilentInhibitor.js';
import { MessageContext } from '../../structures/MessageContext.js';

class TextChannelTracked extends SilentInhibitor {
    async shouldBeBlocked({ message, client }: MessageContext): Promise<string | null> {
        const { channel } = message;

        if (channel.type === 'GUILD_TEXT' || channel.type === 'GUILD_NEWS' || channel.isThread()) {
            await client.master.registry.guilds.insertOrGetIsSpamChannelAsync(channel);
        }

        return null;
    }
}

export = TextChannelTracked;
