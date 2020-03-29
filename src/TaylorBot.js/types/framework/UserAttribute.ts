import AttributeArgumentType = require('./Attribute.js');
import ArgumentParsingError = require('../ArgumentParsingError.js');
import { UserAttribute } from '../../attributes/UserAttribute';
import CommandMessageContext = require('../../commands/CommandMessageContext.js');

class UserAttributeArgumentType extends AttributeArgumentType {
    get id(): string {
        return 'user-attribute';
    }

    parse(val: string, commandContext: CommandMessageContext, arg: any): UserAttribute {
        const attribute = super.parse(val, commandContext, arg);

        if (!(attribute instanceof UserAttribute))
            throw new ArgumentParsingError(`Attribute '${val}' is not a User Attribute.`);

        return attribute;
    }
}

export = UserAttributeArgumentType;
