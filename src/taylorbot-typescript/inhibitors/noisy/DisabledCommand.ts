import { NoisyInhibitor } from '../NoisyInhibitor';
import { CachedCommand } from '../../client/registry/CachedCommand';
import { MessageContext } from '../../structures/MessageContext';

class DisabledCommandInhibitor extends NoisyInhibitor {
    async getBlockedMessage(messageContext: MessageContext, command: CachedCommand): Promise<{ log: string; ui: string } | null> {
        const isDisabled = await messageContext.client.master.registry.commands.insertOrGetIsCommandDisabled(command);
        if (isDisabled) {
            return {
                ui: `You can't use \`${command.name}\` because it is globally disabled right now. Please check back later.`,
                log: 'The command is disabled globally.'
            };
        }

        return null;
    }
}

export = DisabledCommandInhibitor;
