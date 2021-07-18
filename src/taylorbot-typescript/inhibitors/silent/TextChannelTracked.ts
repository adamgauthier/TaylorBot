import { SilentInhibitor } from '../SilentInhibitor.js';
import { MessageContext } from '../../structures/MessageContext.js';

class TextChannelTracked extends SilentInhibitor {
    async shouldBeBlocked({ message, client }: MessageContext): Promise<string | null> {
        const { channel } = message;

        if (channel.type === 'text' || channel.type === 'news') {
            await client.master.registry.guilds.insertOrGetIsSpamChannelAsync(channel);
        }

        return null;
    }
}

export = TextChannelTracked;
