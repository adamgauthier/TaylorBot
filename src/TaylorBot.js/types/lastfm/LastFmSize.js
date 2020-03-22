'use strict';

const PositiveSafeIntegerArgumentType = require('../numbers/PositiveSafeInteger.js');
const ArgumentParsingError = require('../ArgumentParsingError.js');

const MIN_SIZE = 3;
const MAX_SIZE = 5;

class LastFmSizeArgumentType extends PositiveSafeIntegerArgumentType {
    get id() {
        return 'last-fm-size';
    }

    canBeEmpty() {
        return true;
    }

    default() {
        return 3;
    }

    parse(val) {
        const size = super.parse(val);

        if (size < MIN_SIZE || size > MAX_SIZE)
            throw new ArgumentParsingError(`Size must be between ${MIN_SIZE} and ${MAX_SIZE}.`);

        return size;
    }
}

module.exports = LastFmSizeArgumentType;