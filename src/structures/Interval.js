'use strict';

class Interval {
    constructor(intervalTime, enabled = true) {
        if (new.target === Interval) {
            throw new Error(`Can't instantiate abstract ${this.constructor.name} class.`);
        }

        this.intervalTime = intervalTime;
        this.enabled = enabled;
    }

    interval() {
        throw new Error(`${this.constructor.name} doesn't have a ${this.interval.name}() method.`);
    }
}

module.exports = Interval;