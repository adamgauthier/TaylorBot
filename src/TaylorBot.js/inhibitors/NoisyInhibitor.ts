import MessageContext = require('../structures/MessageContext');
import { CachedCommand } from '../client/registry/CachedCommand';

export abstract class NoisyInhibitor {
    abstract getBlockedMessage(messageContext: MessageContext, cachedCommand: CachedCommand, argString: string): Promise<{ log: string; ui: string } | null>;
}
