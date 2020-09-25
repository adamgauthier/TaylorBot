import SafeIntegerArgumentType = require('./SafeInteger.js');
import { ArgumentParsingError } from '../ArgumentParsingError';
import { CommandArgumentInfo, CommandMessageContext } from '../../commands/CommandMessageContext';

class PositiveSafeIntegerArgumentType extends SafeIntegerArgumentType {
    get id(): string {
        return 'positive-safe-integer';
    }

    parse(val: string, commandContext: CommandMessageContext, arg: CommandArgumentInfo): number {
        const number = super.parse(val, commandContext, arg);

        if (number <= 0)
            throw new ArgumentParsingError(`Number '${val}' must be higher than ${0}.`);

        return number;
    }
}

export = PositiveSafeIntegerArgumentType;
