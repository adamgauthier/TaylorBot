'use strict';

const WordArgumentType = require('../base/Word.js');
const ArgumentParsingError = require('../ArgumentParsingError.js');

const Periods = {
    SEVEN_DAYS: '7day',
    THREE_MONTHS: '3month',
    SIX_MONTHS: '6month',
    ONE_YEAR: '12month',
    OVERALL: 'overall'
};

class LastFmPeriodArgumentType extends WordArgumentType {
    get id() {
        return 'last-fm-period';
    }

    canBeEmpty() {
        return true;
    }

    default() {
        return Periods.SEVEN_DAYS;
    }

    parse(val) {
        switch (val.trim()) {
            case '7d':
            case '7day':
            case '7days':
                return Periods.SEVEN_DAYS;
            case '3m':
            case '3month':
            case '3months':
                return Periods.THREE_MONTHS;
            case '6m':
            case '6month':
            case '6months':
                return Periods.SIX_MONTHS;
            case '12m':
            case '12month':
            case '12months':
            case '1y':
            case '1year':
                return Periods.ONE_YEAR;
            case 'overall':
            case 'all':
                return Periods.OVERALL;
            default:
                throw new ArgumentParsingError(
                    `Could not parse '${val}' into a valid Last.fm period. Use one of these: ${Object.values(Periods).map(p => `\`${p}\``).join()}.`
                );
        }
    }
}

module.exports = LastFmPeriodArgumentType;