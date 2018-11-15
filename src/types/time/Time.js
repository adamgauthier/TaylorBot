'use strict';

const chrono = require('chrono-node');

const TextArgumentType = require('../base/Text.js');
const ArgumentParsingError = require('../ArgumentParsingError.js');

class TimeArgumentType extends TextArgumentType {
    get id() {
        return 'time';
    }

    parse(val) {
        const date = chrono.parseDate(val);

        if (date === null)
            throw new ArgumentParsingError(`Could not parse date from '${val}'.`);

        return date;
    }
}

module.exports = TimeArgumentType;