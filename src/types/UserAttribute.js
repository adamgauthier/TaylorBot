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
        const userAttributes = client.master.registry.attributes.filter(
            attribute => attribute instanceof UserAttribute
        );

        if (!userAttributes.has(key))
            throw new ArgumentParsingError(`Member Attribute '${key}' doesn't exist.`);

        return userAttributes.get(key);
    }
}

module.exports = UserAttributeArgumentType;