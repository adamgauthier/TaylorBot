import WordArgumentType = require('../base/Word');
import { ArgumentParsingError } from '../ArgumentParsingError';
import { CommandArgumentInfo, CommandMessageContext } from '../../commands/CommandMessageContext';
import { MemberAttribute } from '../../attributes/MemberAttribute.js';
import { UserAttribute } from '../../attributes/UserAttribute.js';

class AttributeArgumentType extends WordArgumentType {
    get id(): string {
        return 'attribute';
    }

    parse(val: string, { client }: CommandMessageContext, arg: CommandArgumentInfo): MemberAttribute | UserAttribute | { id: 'lastfm'; canSet: true; list: null } {
        if (val.toLowerCase() === 'fm' || val.toLowerCase() === 'lastfm') {
            return { id: 'lastfm', canSet: true, list: null };
        }

        const attribute = client.master.registry.attributes.resolve(val);

        if (!attribute)
            throw new ArgumentParsingError(`Attribute '${val}' doesn't exist.`);

        return attribute;
    }
}

export = AttributeArgumentType;
