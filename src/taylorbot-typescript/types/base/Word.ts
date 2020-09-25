import { ArgumentType } from '../ArgumentType';
import { CommandArgumentInfo, CommandMessageContext } from '../../commands/CommandMessageContext';

class WordArgumentType extends ArgumentType {
    constructor() {
        super({
            includesSpaces: false,
            includesNewLines: false,
            mustBeQuoted: false
        });
    }

    get id(): string {
        return 'word';
    }

    parse(val: string, commandContext: CommandMessageContext, arg: CommandArgumentInfo): any {
        return val;
    }
}

export = WordArgumentType;
