'use strict';

const AttributeArgumentType = require('./Attribute.js');
const ArgumentParsingError = require('../ArgumentParsingError.js');

class ListableAttributeArgumentType extends AttributeArgumentType {
    get id() {
        return 'listable-attribute';
    }

    parse(val, commandContext, arg) {
        const attribute = super.parse(val, commandContext, arg);

        if (!attribute.canList)
            throw new ArgumentParsingError(`Attribute '${val}' can't be listed.`);

        return attribute;
    }
}

module.exports = ListableAttributeArgumentType;