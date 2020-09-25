import { ArgumentParsingError } from '../ArgumentParsingError';
import UserAttributeArgumentType = require('./UserAttribute');
import { UserAttribute } from '../../attributes/UserAttribute';
import { CommandMessageContext, CommandArgumentInfo } from '../../commands/CommandMessageContext';

class SettableAttributeArgumentType extends UserAttributeArgumentType {
    get id(): string {
        return 'settable-attribute';
    }

    parse(val: string, commandContext: CommandMessageContext, arg: CommandArgumentInfo): UserAttribute {
        const attribute = super.parse(val, commandContext, arg);

        if (!attribute.canSet)
            throw new ArgumentParsingError(`Attribute '${val}' can't be changed.`);

        return attribute;
    }
}

export = SettableAttributeArgumentType;
