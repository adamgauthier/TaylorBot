'use strict';

class Interval {
    constructor(intervalTime, enabled = true) {
        if (new.target === Interval) {
            throw new Error(`Can't instantiate abstract Interval class.`);
        }

        this.intervalTime = intervalTime;
        this.enabled = enabled;
    }

    interval() {
        throw new Error(`${this.constructor.name} doesn't have a interval() method.`);
    }
}

module.exports = Interval;