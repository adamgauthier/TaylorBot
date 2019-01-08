'use strict';

const WordArgumentType = require('../base/Word.js');
const ArgumentParsingError = require('../ArgumentParsingError.js');
const TaypointAmount = require('../../modules/points/TaypointAmount.js');

const MIN_AMOUNT = 1;

class TaypointAmountArgumentType extends WordArgumentType {
    get id() {
        return 'taypoint-amount';
    }

    parse(val) {
        switch (val) {
            case 'all':
                return new TaypointAmount({ divisor: 1 });
            case 'half':
                return new TaypointAmount({ divisor: 2 });
            case 'third':
                return new TaypointAmount({ divisor: 3 });
            case 'fourth':
                return new TaypointAmount({ divisor: 4 });
        }

        const number = Number.parseInt(val);

        if (Number.isNaN(number))
            throw new ArgumentParsingError(`Could not parse '${val}' into a valid taypoint amount.`);

        if (number < MIN_AMOUNT)
            throw new ArgumentParsingError(`Taypoint amount '${number}' must be higher than ${MIN_AMOUNT}.`);

        return new TaypointAmount({ count: number });
    }
}

module.exports = TaypointAmountArgumentType;