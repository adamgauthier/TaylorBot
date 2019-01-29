'use strict';

const IntegerArgumentType = require('./Integer.js');
const ArgumentParsingError = require('../ArgumentParsingError.js');

const MIN_VALUE = 0;
const MAX_VALUE = 4294967295;

class UnsignedInteger32ArgumentType extends IntegerArgumentType {
    get id() {
        return 'unsigned-integer-32';
    }

    parse(val) {
        const number = super.parse(val);

        if (number < MIN_VALUE)
            throw new ArgumentParsingError(`Number '${val}' must be equal to or greater than ${MIN_VALUE}.`);

        if (number > MAX_VALUE)
            throw new ArgumentParsingError(`Number '${val}' must be equal to or less than ${MAX_VALUE}.`);

        return number;
    }
}

module.exports = UnsignedInteger32ArgumentType;