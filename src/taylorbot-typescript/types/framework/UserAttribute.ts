import AttributeArgumentType = require('./Attribute.js');
import { ArgumentParsingError } from '../ArgumentParsingError';
import { UserAttribute } from '../../attributes/UserAttribute';
import { CommandMessageContext, CommandArgumentInfo } from '../../commands/CommandMessageContext';

class UserAttributeArgumentType extends AttributeArgumentType {
    get id(): string {
        return 'user-attribute';
    }

    parse(val: string, commandContext: CommandMessageContext, arg: CommandArgumentInfo): UserAttribute {
        const attribute = super.parse(val, commandContext, arg);

        if (!(attribute instanceof UserAttribute) && attribute.id !== 'lastfm')
            throw new ArgumentParsingError(`Attribute '${val}' is not a User Attribute.`);

        return attribute as UserAttribute;
    }
}

export = UserAttributeArgumentType;
