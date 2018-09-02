'use strict';

const ArgumentType = require('../structures/ArgumentType.js');
const ArgumentParsingError = require('../structures/ArgumentParsingError.js');
const MemberAttribute = require('../attributes/MemberAttribute.js');

class MemberAttributeArgumentType extends ArgumentType {
    get id() {
        return 'member-attribute';
    }

    parse(val, { client }) {
        const key = val.toLowerCase();
        const { attributes } = client.master.registry;

        if (!attributes.has(key))
            throw new ArgumentParsingError(`Member Attribute '${key}' doesn't exist.`);

        const attribute = attributes.get(key);

        if (!(attribute instanceof MemberAttribute))
            throw new ArgumentParsingError(`Attribute '${key}' is not a Member Attribute.`);

        return attribute;
    }
}

module.exports = MemberAttributeArgumentType;