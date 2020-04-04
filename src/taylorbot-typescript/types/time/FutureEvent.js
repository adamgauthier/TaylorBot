'use strict';

const moment = require('moment');
const Sherlock = require('sherlockjs');

const TextArgumentType = require('../base/Text.js');
const ArgumentParsingError = require('../ArgumentParsingError.js');

class FutureEventArgumentType extends TextArgumentType {
    get id() {
        return 'future-event';
    }

    parse(val) {
        const sherlocked = Sherlock.parse(val);

        if (sherlocked.startDate !== null && sherlocked.eventTitle !== '') {
            if (moment(sherlocked.startDate).isAfter()) {
                return sherlocked;
            }
            throw new ArgumentParsingError(`Event '${val}' is not in the future.`);
        }

        throw new ArgumentParsingError(`Could not parse event from '${val}'.`);
    }
}

module.exports = FutureEventArgumentType;