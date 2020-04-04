'use strict';

const WordArgumentType = require('../base/Word.js');
const ArgumentParsingError = require('../ArgumentParsingError.js');

class AttributeArgumentType extends WordArgumentType {
    get id() {
        return 'attribute';
    }

    parse(val, { client }, arg) {
        const attribute = client.master.registry.attributes.resolve(val);

        if (!attribute)
            throw new ArgumentParsingError(`Attribute '${val}' doesn't exist.`);

        return attribute;
    }
}

module.exports = AttributeArgumentType;
