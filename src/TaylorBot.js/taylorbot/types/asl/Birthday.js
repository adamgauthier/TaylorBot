'use strict';

const moment = require('moment');
const Sherlock = require('sherlockjs');

const TextArgumentType = require('../base/Text.js');
const ArgumentParsingError = require('../ArgumentParsingError.js');

const MIN_AGE = 13;
const MAX_AGE = 115;

class BirthdayArgumentType extends TextArgumentType {
    get id() {
        return 'birthday';
    }

    parse(val) {
        const date = this.parseDate(val.trim());
        const age = date.diff(moment.utc(), 'years');

        // Same year assumes no year
        if (age === 0) {
            date.year(1804);
        }
        else {
            if (age > -MIN_AGE)
                throw new ArgumentParsingError(`Age must be higher or equal to ${MIN_AGE} years old.`);

            if (age < -MAX_AGE)
                throw new ArgumentParsingError(`Age must be lower or equal to ${MAX_AGE} years old.`);
        }

        return date;
    }

    parseDate(text) {
        const withYear = moment.utc(text, 'YYYY-MM-DD', true);
        if (withYear.isValid())
            return withYear;

        const withoutYear = moment.utc(text, 'MM-DD', true);
        if (withoutYear.isValid())
            return withoutYear;

        const withoutYearReversed = moment.utc(text, 'DD-MM', true);
        if (withoutYearReversed.isValid())
            return withoutYearReversed;

        const sherlocked = Sherlock.parse(text);

        if (sherlocked.startDate !== null) {
            const converted = moment.utc(moment(sherlocked.startDate).format('YYYY-MM-DD'), 'YYYY-MM-DD', true);
            if (converted.isValid())
                return converted;
        }

        throw new ArgumentParsingError(`Could not parse valid birthday from '${text}', please use YYYY-MM-DD format.`);
    }
}

module.exports = BirthdayArgumentType;