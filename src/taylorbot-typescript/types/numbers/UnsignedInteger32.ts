import IntegerArgumentType = require('./Integer.js');
import { ArgumentParsingError } from '../ArgumentParsingError';
import { CommandArgumentInfo, CommandMessageContext } from '../../commands/CommandMessageContext';

const MIN_VALUE = 0;
const MAX_VALUE = 4294967295;

class UnsignedInteger32ArgumentType extends IntegerArgumentType {
    get id(): string {
        return 'unsigned-integer-32';
    }

    parse(val: string, commandContext: CommandMessageContext, arg: CommandArgumentInfo): number {
        const number = super.parse(val, commandContext, arg);

        if (number < MIN_VALUE)
            throw new ArgumentParsingError(`Number '${val}' must be equal to or greater than ${MIN_VALUE}.`);

        if (number > MAX_VALUE)
            throw new ArgumentParsingError(`Number '${val}' must be equal to or less than ${MAX_VALUE}.`);

        return number;
    }
}

export = UnsignedInteger32ArgumentType;
