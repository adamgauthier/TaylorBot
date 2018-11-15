'use strict';

const UserAttributeArgumentType = require('./UserAttribute.js');
const ArgumentParsingError = require('../ArgumentParsingError.js');

class SettableAttributeArgumentType extends UserAttributeArgumentType {
    get id() {
        return 'settable-attribute';
    }

    parse(val, commandContext, arg) {
        const attribute = super.parse(val, commandContext, arg);

        if (!attribute.canSet)
            throw new ArgumentParsingError(`Attribute '${val}' can't be changed.`);

        return attribute;
    }
}

module.exports = SettableAttributeArgumentType;