import { NoisyInhibitor } from '../NoisyInhibitor';
import { CachedCommand } from '../../client/registry/CachedCommand';
import { MessageContext } from '../../structures/MessageContext';

class GuildOnlyInhibitor extends NoisyInhibitor {
    getBlockedMessage({ message }: MessageContext, command: CachedCommand): Promise<{ log: string; ui: string } | null> {
        const { channel } = message;

        if (command.command.guildOnly && channel.type !== 'text') {
            return Promise.resolve({
                ui: `You can't use \`${command.name}\` because it can only be used in a server.`,
                log: 'The command is marked as guild only.'
            });
        }

        return Promise.resolve(null);
    }
}

export = GuildOnlyInhibitor;
