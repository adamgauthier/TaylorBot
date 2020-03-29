import { SilentInhibitor } from '../SilentInhibitor';
import MessageContext = require('../../structures/MessageContext');

class AnsweredCooldownInhibitor extends SilentInhibitor {
    async shouldBeBlocked(messageContext: MessageContext): Promise<string | null> {
        const { author } = messageContext.message;
        const { onGoingCommands } = messageContext.client.master.registry;

        if (messageContext.wasOnGoingCommandAdded) {
            return null;
        }

        const hasAnyOngoingCommand = await onGoingCommands.hasAnyOngoingCommandAsync(author);

        if (hasAnyOngoingCommand) {
            return 'They have an ongoing command.';
        }
        else {
            await onGoingCommands.addOngoingCommandAsync(author);
            messageContext.wasOnGoingCommandAdded = true;
            return null;
        }
    }
}

export = AnsweredCooldownInhibitor;
