'use strict';

const WordArgumentType = require('./Word.js');
const ArgumentParsingError = require('../structures/ArgumentParsingError.js');

class AttributeArgumentType extends WordArgumentType {
    get id() {
        return 'attribute';
    }

    parse(val, { client }) {
        const attribute = client.master.registry.attributes.resolve(val);

        if (!attribute)
            throw new ArgumentParsingError(`Attribute '${val}' doesn't exist.`);

        return attribute;
    }
}

module.exports = AttributeArgumentType;