import WordArgumentType = require('../base/Word');
import { ArgumentParsingError } from '../ArgumentParsingError';
import { CommandArgumentInfo, CommandMessageContext } from '../../commands/CommandMessageContext';

class IntegerArgumentType extends WordArgumentType {
    get id(): string {
        return 'integer';
    }

    parse(val: string, commandContext: CommandMessageContext, arg: CommandArgumentInfo): number {
        const number = Number.parseInt(val);

        if (Number.isNaN(number))
            throw new ArgumentParsingError(`Could not parse '${val}' into a valid number.`);

        return number;
    }
}

export = IntegerArgumentType;
