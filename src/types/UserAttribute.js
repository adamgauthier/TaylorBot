'use strict';

const ArgumentType = require('../structures/ArgumentType.js');
const ArgumentParsingError = require('../structures/ArgumentParsingError.js');
const UserAttribute = require('../attributes/UserAttribute.js');

class UserAttributeArgumentType extends ArgumentType {
    get id() {
        return 'user-attribute';
    }

    parse(val, { client }) {
        const key = val.toLowerCase();
        const { attributes } = client.master.registry;

        if (!attributes.has(key))
            throw new ArgumentParsingError(`User Attribute '${key}' doesn't exist.`);

        const attribute = attributes.get(key);

        if (!(attribute instanceof UserAttribute))
            throw new ArgumentParsingError(`Attribute '${key}' is not a User Attribute.`);

        return attribute;
    }
}

module.exports = UserAttributeArgumentType;