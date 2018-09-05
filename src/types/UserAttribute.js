'use strict';

const WordArgumentType = require('./Word.js');
const ArgumentParsingError = require('../structures/ArgumentParsingError.js');
const UserAttribute = require('../attributes/UserAttribute.js');

class UserAttributeArgumentType extends WordArgumentType {
    get id() {
        return 'user-attribute';
    }

    parse(val, { client }) {
        const attribute = client.master.registry.attributes.resolve(val);

        if (!attribute)
            throw new ArgumentParsingError(`User Attribute '${val}' doesn't exist.`);

        if (!(attribute instanceof UserAttribute))
            throw new ArgumentParsingError(`Attribute '${val}' is not a User Attribute.`);

        return attribute;
    }
}

module.exports = UserAttributeArgumentType;