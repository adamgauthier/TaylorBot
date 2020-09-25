import { ArgumentInfo, CommandArgumentInfo, CommandMessageContext } from '../commands/CommandMessageContext';
import { MessageContext } from '../structures/MessageContext';

export abstract class ArgumentType {
    includesSpaces: boolean;
    includesNewLines: boolean;
    mustBeQuoted: boolean;

    constructor({ includesSpaces, includesNewLines, mustBeQuoted }: { includesSpaces: boolean; includesNewLines: boolean; mustBeQuoted: boolean }) {
        this.includesSpaces = includesSpaces;
        this.includesNewLines = includesNewLines;
        this.mustBeQuoted = mustBeQuoted;
    }

    get id(): string {
        return '';
    }

    canBeEmpty(messageContext: MessageContext, info: ArgumentInfo): boolean {
        return false;
    }

    default(commandContext: CommandMessageContext, arg: ArgumentInfo): any {
        throw new Error(`Not implemented.`);
    }

    abstract parse(val: string, commandContext: CommandMessageContext, arg: CommandArgumentInfo): any;
}
