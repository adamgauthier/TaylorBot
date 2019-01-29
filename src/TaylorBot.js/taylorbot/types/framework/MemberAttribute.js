'use strict';

const AttributeArgumentType = require('./Attribute.js');
const ArgumentParsingError = require('../ArgumentParsingError.js');
const MemberAttribute = require('../../attributes/MemberAttribute.js');

class MemberAttributeArgumentType extends AttributeArgumentType {
    get id() {
        return 'member-attribute';
    }

    parse(val, commandContext, arg) {
        const attribute = super.parse(val, commandContext, arg);

        if (!(attribute instanceof MemberAttribute))
            throw new ArgumentParsingError(`Attribute '${val}' is not a Member Attribute.`);

        return attribute;
    }
}

module.exports = MemberAttributeArgumentType;