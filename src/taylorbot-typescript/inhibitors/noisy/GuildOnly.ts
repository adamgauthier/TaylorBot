import { NoisyInhibitor } from '../NoisyInhibitor';
import { CachedCommand } from '../../client/registry/CachedCommand';
import { MessageContext } from '../../structures/MessageContext';

class GuildOnlyInhibitor extends NoisyInhibitor {
    getBlockedMessage(context: MessageContext, command: CachedCommand): Promise<{ log: string; ui: string } | null> {
        if (command.command.guildOnly && !context.isGuild) {
            return Promise.resolve({
                ui: `You can't use \`${command.name}\` because it can only be used in a server.`,
                log: 'The command is marked as guild only.'
            });
        }

        return Promise.resolve(null);
    }
}

export = GuildOnlyInhibitor;
