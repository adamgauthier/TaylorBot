import AttributeArgumentType = require('./Attribute.js');
import { ArgumentParsingError } from '../ArgumentParsingError';
import { CommandArgumentInfo, CommandMessageContext } from '../../commands/CommandMessageContext';
import { Attribute } from '../../attributes/Attribute';
import WordArgumentType = require('../base/Word');

class ListableAttributeArgumentType extends WordArgumentType {
    readonly #attributeArgumentType = new AttributeArgumentType();

    get id(): string {
        return 'listable-attribute';
    }

    parse(val: string, commandContext: CommandMessageContext, arg: CommandArgumentInfo): Attribute {
        const attribute = this.#attributeArgumentType.parse(val, commandContext, arg);

        if (typeof attribute.list == 'string')
            throw new ArgumentParsingError(`This command has been removed. Please use ${attribute.list} instead.`);

        if (attribute.list === null)
            throw new ArgumentParsingError(`Attribute '${val}' can't be listed.`);

        return attribute;
    }
}

export = ListableAttributeArgumentType;
