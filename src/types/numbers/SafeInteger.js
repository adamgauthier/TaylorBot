'use strict';

const WordArgumentType = require('../base/Word.js');
const ArgumentParsingError = require('../ArgumentParsingError.js');

class SafeIntegerArgumentType extends WordArgumentType {
    get id() {
        return 'safe-integer';
    }

    parse(val) {
        const number = Number.parseInt(val);

        if (Number.isNaN(number))
            throw new ArgumentParsingError(`Could not parse '${val}' into a valid number.`);

        if (number > Number.MAX_SAFE_INTEGER)
            throw new ArgumentParsingError(`Number '${val}' must be equal to or less than ${Number.MAX_SAFE_INTEGER}.`);

        return number;
    }
}

module.exports = SafeIntegerArgumentType;