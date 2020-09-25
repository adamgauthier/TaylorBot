import { ArgumentType } from '../ArgumentType';
import { CommandArgumentInfo, CommandMessageContext } from '../../commands/CommandMessageContext';

class MultilineTextArgumentType extends ArgumentType {
    constructor() {
        super({
            includesSpaces: true,
            includesNewLines: true,
            mustBeQuoted: false
        });
    }

    get id(): string {
        return 'multiline-text';
    }

    parse(val: string, commandContext: CommandMessageContext, arg: CommandArgumentInfo): any {
        return val;
    }
}

export = MultilineTextArgumentType;
