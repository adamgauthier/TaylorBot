'use strict';

class Interval {
    constructor(intervalTime, interval, enabled = true) {
        if (new.target === Interval) {
            throw new Error(`Can't instantiate abstract Interval class.`);
        }

        this.intervalTime = intervalTime;
        this.interval = interval;
        this.enabled = enabled;
    }
}

module.exports = Interval;