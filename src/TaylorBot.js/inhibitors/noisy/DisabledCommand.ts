import { NoisyInhibitor } from '../NoisyInhibitor';
import { TaylorBotClient } from '../../client/TaylorBotClient';
import { CachedCommand } from '../../client/registry/CachedCommand';

class DisabledCommandInhibitor extends NoisyInhibitor {
    async getBlockedMessage(messageContext: { client: TaylorBotClient }, command: CachedCommand): Promise<{ log: string; ui: string } | null> {
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
