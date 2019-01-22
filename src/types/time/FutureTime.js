'use strict';

const chrono = require('chrono-node');

const TextArgumentType = require('../base/Text.js');
const ArgumentParsingError = require('../ArgumentParsingError.js');

class FutureTimeArgumentType extends TextArgumentType {
    get id() {
        return 'future-time';
    }

    parse(val) {
        const results = chrono.parse(val, new Date(), { forwardDate: true });

        if (results.length > 0) {
            const moment = results[0].start.moment();

            if (moment.isAfter()) {
                return moment;
            }
        }

        throw new ArgumentParsingError(`Could not parse future date from '${val}'.`);
    }
}

module.exports = FutureTimeArgumentType;