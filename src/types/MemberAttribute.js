'use strict';

const WordArgumentType = require('./Word.js');
const ArgumentParsingError = require('../structures/ArgumentParsingError.js');
const MemberAttribute = require('../attributes/MemberAttribute.js');

class MemberAttributeArgumentType extends WordArgumentType {
    get id() {
        return 'member-attribute';
    }

    parse(val, { client }) {
        const attribute = client.master.registry.attributes.resolve(val);

        if (!attribute)
            throw new ArgumentParsingError(`Member Attribute '${val}' doesn't exist.`);

        if (!(attribute instanceof MemberAttribute))
            throw new ArgumentParsingError(`Attribute '${val}' is not a Member Attribute.`);

        return attribute;
    }
}

module.exports = MemberAttributeArgumentType;