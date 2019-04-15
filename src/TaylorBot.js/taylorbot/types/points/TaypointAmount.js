'use strict';

const PositiveSafeIntegerArgumentType = require('../numbers/PositiveSafeInteger.js');
const ArgumentParsingError = require('../ArgumentParsingError.js');
const TaypointAmount = require('../../modules/points/TaypointAmount.js');
const StringUtil = require('../../modules/StringUtil.js');

class TaypointAmountArgumentType extends PositiveSafeIntegerArgumentType {
    get id() {
        return 'taypoint-amount';
    }

    async parse(val, { client, message }) {
        switch (val.toLowerCase()) {
            case 'all':
                return new TaypointAmount({ divisor: 1 });
            case 'half':
                return new TaypointAmount({ divisor: 2 });
            case 'third':
                return new TaypointAmount({ divisor: 3 });
            case 'fourth':
                return new TaypointAmount({ divisor: 4 });
        }

        const number = super.parse(val);

        const { taypoint_count, has_enough } = await client.master.database.users.hasEnoughTaypointCount(message.author, number);

        if (!has_enough)
            throw new ArgumentParsingError(`You can't spend ${StringUtil.plural(number, 'taypoint', '**')}, you only have **${taypoint_count}**.`);

        return new TaypointAmount({ count: number });
    }
}

module.exports = TaypointAmountArgumentType;