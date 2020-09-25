import IntegerArgumentType = require('./Integer.js');
import { ArgumentParsingError } from '../ArgumentParsingError';
import { CommandArgumentInfo, CommandMessageContext } from '../../commands/CommandMessageContext';

class SafeIntegerArgumentType extends IntegerArgumentType {
    get id(): string {
        return 'safe-integer';
    }

    parse(val: string, commandContext: CommandMessageContext, arg: CommandArgumentInfo): number {
        const number = super.parse(val, commandContext, arg);

        if (number < Number.MIN_SAFE_INTEGER)
            throw new ArgumentParsingError(`Number '${val}' must be equal to or greater than ${Number.MIN_SAFE_INTEGER}.`);

        if (number > Number.MAX_SAFE_INTEGER)
            throw new ArgumentParsingError(`Number '${val}' must be equal to or less than ${Number.MAX_SAFE_INTEGER}.`);

        return number;
    }
}

export = SafeIntegerArgumentType;
