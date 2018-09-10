'use strict';

const AttributeArgumentType = require('./Attribute.js');
const ArgumentParsingError = require('../structures/ArgumentParsingError.js');
const UserAttribute = require('../attributes/UserAttribute.js');

class UserAttributeArgumentType extends AttributeArgumentType {
    get id() {
        return 'user-attribute';
    }

    parse(val, commandContext, arg) {
        const attribute = super.parse(val, commandContext, arg);

        if (!(attribute instanceof UserAttribute))
            throw new ArgumentParsingError(`Attribute '${val}' is not a User Attribute.`);

        return attribute;
    }
}

module.exports = UserAttributeArgumentType;