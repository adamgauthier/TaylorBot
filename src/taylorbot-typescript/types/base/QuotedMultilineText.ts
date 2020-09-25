import { ArgumentType } from '../ArgumentType';
import { CommandArgumentInfo, CommandMessageContext } from '../../commands/CommandMessageContext';

class QuotedMultilineTextArgumentType extends ArgumentType {
    constructor() {
        super({
            includesSpaces: true,
            includesNewLines: true,
            mustBeQuoted: true
        });
    }

    get id(): string {
        return 'quoted-multiline-text';
    }

    parse(val: string, commandContext: CommandMessageContext, arg: CommandArgumentInfo): any {
        return val;
    }
}

export = QuotedMultilineTextArgumentType;
