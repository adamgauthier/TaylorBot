'use strict';

const WordArgumentType = require('../base/Word.js');
const ArgumentParsingError = require('../ArgumentParsingError.js');

class IntegerArgumentType extends WordArgumentType {
    get id() {
        return 'integer';
    }

    parse(val) {
        const number = Number.parseInt(val);

        if (Number.isNaN(number))
            throw new ArgumentParsingError(`Could not parse '${val}' into a valid number.`);

        return number;
    }
}

module.exports = IntegerArgumentType;