'use strict';

const PositiveSafeIntegerArgumentType = require('../numbers/PositiveSafeInteger.js');
const ArgumentParsingError = require('../ArgumentParsingError.js');

const MIN_AGE = 13;
const MAX_AGE = 115;

class AgeArgumentType extends PositiveSafeIntegerArgumentType {
    get id() {
        return 'age';
    }

    parse(val) {
        const age = super.parse(val);

        if (age < MIN_AGE)
            throw new ArgumentParsingError(`Age must be higher or equal to ${MIN_AGE} years old.`);

        if (age > MAX_AGE)
            throw new ArgumentParsingError(`Age must be lower or equal to ${MAX_AGE} years old.`);

        return age;
    }
}

module.exports = AgeArgumentType;