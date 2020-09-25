import { ArgumentType } from '../ArgumentType';
import { CommandArgumentInfo, CommandMessageContext } from '../../commands/CommandMessageContext';

class TextArgumentType extends ArgumentType {
    constructor() {
        super({
            includesSpaces: true,
            includesNewLines: false,
            mustBeQuoted: false
        });
    }

    get id(): string {
        return 'text';
    }

    parse(val: string, commandContext: CommandMessageContext, arg: CommandArgumentInfo): any {
        return val;
    }
}

export = TextArgumentType;
