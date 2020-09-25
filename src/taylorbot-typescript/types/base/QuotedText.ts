import { ArgumentType } from '../ArgumentType';
import { CommandArgumentInfo, CommandMessageContext } from '../../commands/CommandMessageContext';

class QuotedTextArgumentType extends ArgumentType {
    constructor() {
        super({
            includesSpaces: true,
            includesNewLines: false,
            mustBeQuoted: true,
        });
    }

    get id(): string {
        return 'quoted-text';
    }

    parse(val: string, commandContext: CommandMessageContext, arg: CommandArgumentInfo): any {
        return val;
    }
}

export = QuotedTextArgumentType;
