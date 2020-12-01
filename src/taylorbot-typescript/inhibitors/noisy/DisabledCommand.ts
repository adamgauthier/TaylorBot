import { NoisyInhibitor } from '../NoisyInhibitor';
import { CachedCommand } from '../../client/registry/CachedCommand';
import { MessageContext } from '../../structures/MessageContext';

class DisabledCommandInhibitor extends NoisyInhibitor {
    async getBlockedMessage(messageContext: MessageContext, command: CachedCommand): Promise<{ log: string; ui: string } | null> {
        const disabledMessage = await messageContext.client.master.registry.commands.insertOrGetCommandDisabledMessage(command);
        if (disabledMessage !== '') {
            return {
                ui: [
                    `You can't use \`${command.name}\` because it is globally disabled right now.`,
                    disabledMessage
                ].join('\n'),
                log: 'The command is disabled globally.'
            };
        }

        return null;
    }
}

export = DisabledCommandInhibitor;
