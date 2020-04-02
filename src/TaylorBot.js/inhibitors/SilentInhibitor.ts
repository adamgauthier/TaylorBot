import { MessageContext } from '../structures/MessageContext';
import { CachedCommand } from '../client/registry/CachedCommand';

export abstract class SilentInhibitor {
    abstract shouldBeBlocked(messageContext: MessageContext, cachedCommand: CachedCommand, argString: string): Promise<string | null>;
}
