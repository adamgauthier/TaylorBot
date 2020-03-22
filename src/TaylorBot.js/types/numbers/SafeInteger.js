'use strict';

const IntegerArgumentType = require('./Integer.js');
const ArgumentParsingError = require('../ArgumentParsingError.js');

class SafeIntegerArgumentType extends IntegerArgumentType {
    get id() {
        return 'safe-integer';
    }

    parse(val) {
        const number = super.parse(val);

        if (number < Number.MIN_SAFE_INTEGER)
            throw new ArgumentParsingError(`Number '${val}' must be equal to or greater than ${Number.MIN_SAFE_INTEGER}.`);

        if (number > Number.MAX_SAFE_INTEGER)
            throw new ArgumentParsingError(`Number '${val}' must be equal to or less than ${Number.MAX_SAFE_INTEGER}.`);

        return number;
    }
}

module.exports = SafeIntegerArgumentType;