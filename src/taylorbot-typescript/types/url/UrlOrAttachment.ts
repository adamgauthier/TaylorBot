import { ArgumentInfo, CommandMessageContext } from '../../commands/CommandMessageContext';
import { MessageContext } from '../../structures/MessageContext';
import UrlArgumentType = require('./Url');

class UrlOrAttachmentArgumentType extends UrlArgumentType {
    get id(): string {
        return 'url-or-attachment';
    }

    canBeEmpty(messageContext: MessageContext, info: ArgumentInfo): boolean {
        return messageContext.message.attachments.size > 0;
    }

    default({ message }: CommandMessageContext): string {
        return message.attachments.first()!.url;
    }
}

export = UrlOrAttachmentArgumentType;
