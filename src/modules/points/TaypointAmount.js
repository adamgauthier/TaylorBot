'use strict';

class TaypointAmount {
    constructor({ count = undefined, divisor = undefined }) {
        if ((count !== undefined && divisor !== undefined) || (count === undefined && divisor === undefined))
            throw new Error('You must provide either a count or a divisor.');

        this.count = count;
        this.divisor = divisor;
    }

    get isRelative() {
        return this.divisor !== undefined;
    }
}

module.exports = TaypointAmount;