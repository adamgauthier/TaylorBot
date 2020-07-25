'use strict';

const WordArgumentType = require('../base/Word.js');
const ArgumentParsingError = require('../ArgumentParsingError.js');

class AttributeArgumentType extends WordArgumentType {
    get id() {
        return 'attribute';
    }

    parse(val, { client }, arg) {
        if (val.toLowerCase() === 'fm' || val.toLowerCase() === 'lastfm') {
            return { id: 'lastfm', canSet: true };
        }

        const attribute = client.master.registry.attributes.resolve(val);

        if (!attribute)
            throw new ArgumentParsingError(`Attribute '${val}' doesn't exist.`);

        return attribute;
    }
}

module.exports = AttributeArgumentType;
