import AttributeArgumentType = require('./Attribute.js');
import { ArgumentParsingError } from '../ArgumentParsingError';
import { MemberAttribute } from '../../attributes/MemberAttribute';
import { CommandMessageContext, CommandArgumentInfo } from '../../commands/CommandMessageContext';

class MemberAttributeArgumentType extends AttributeArgumentType {
    get id(): string {
        return 'member-attribute';
    }

    parse(val: string, commandContext: CommandMessageContext, arg: CommandArgumentInfo): MemberAttribute {
        const attribute = super.parse(val, commandContext, arg);

        if (!(attribute instanceof MemberAttribute))
            throw new ArgumentParsingError(`Attribute '${val}' is not a Member Attribute.`);

        return attribute;
    }
}

export = MemberAttributeArgumentType;
