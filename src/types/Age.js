'use strict';

const TextArgumentType = require('./Text.js');
const ArgumentParsingError = require('../structures/ArgumentParsingError.js');

const MIN_AGE = 13;
const MAX_AGE = 115;

class AgeArgumentType extends TextArgumentType {
    get id() {
        return 'age';
    }

    parse(val) {
        const age = Number.parseInt(val);

        if (Number.isNaN(age))
            throw new ArgumentParsingError(`Could not parse '${val}' into a valid number.`);

        if (age < MIN_AGE)
            throw new ArgumentParsingError(`Age must be higher or equal to ${MIN_AGE} years old.`);

        if (age > MAX_AGE)
            throw new ArgumentParsingError(`Age must be lower or equal to ${MAX_AGE} years old.`);

        return age;
    }
}

module.exports = AgeArgumentType;