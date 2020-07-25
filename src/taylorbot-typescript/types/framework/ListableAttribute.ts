import AttributeArgumentType = require('./Attribute.js');
import ArgumentParsingError = require('../ArgumentParsingError.js');
import { CommandArgumentInfo, CommandMessageContext } from '../../commands/CommandMessageContext';
import { Attribute } from '../../attributes/Attribute';

class ListableAttributeArgumentType extends AttributeArgumentType {
    get id(): string {
        return 'listable-attribute';
    }

    parse(val: string, commandContext: CommandMessageContext, arg: CommandArgumentInfo): Attribute {
        const attribute = super.parse(val, commandContext, arg);

        if (attribute.list === null || attribute.id === 'lastfm')
            throw new ArgumentParsingError(`Attribute '${val}' can't be listed.`);

        return attribute;
    }
}

export = ListableAttributeArgumentType;
