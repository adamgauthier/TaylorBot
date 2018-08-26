'use strict';

const ArgumentType = require('../structures/ArgumentType.js');
const ArgumentParsingError = require('../structures/ArgumentParsingError.js');

class MemberAttributeArgumentType extends ArgumentType {
    get id() {
        return 'member-attribute';
    }

    parse(val, { client }) {
        const key = val.toLowerCase();
        const { attributes } = client.master.registry;

        if (!attributes.has(key))
            throw new ArgumentParsingError(`Member Attribute '${key}' doesn't exist.`);

        return attributes.get(key);
    }
}

module.exports = MemberAttributeArgumentType;