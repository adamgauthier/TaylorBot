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

    toString() {
        return `(${this.isRelative ? `divisor: ${this.divisor}` : `count: ${this.count}`})`;
    }
}

module.exports = TaypointAmount;