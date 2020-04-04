'use strict';

const SafeIntegerArgumentType = require('./SafeInteger.js');
const ArgumentParsingError = require('../ArgumentParsingError.js');

class PositiveSafeIntegerArgumentType extends SafeIntegerArgumentType {
    get id() {
        return 'positive-safe-integer';
    }

    parse(val) {
        const number = super.parse(val);

        if (number <= 0)
            throw new ArgumentParsingError(`Number '${val}' must be higher than ${0}.`);

        return number;
    }
}

module.exports = PositiveSafeIntegerArgumentType;