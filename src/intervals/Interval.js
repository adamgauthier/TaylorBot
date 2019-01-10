'use strict';

class Interval {
    constructor({ id, intervalMs, enabled = true }) {
        if (new.target === Interval) {
            throw new Error(`Can't instantiate abstract ${this.constructor.name} class.`);
        }

        this.id = id;
        this.intervalMs = intervalMs;
        this.enabled = enabled;
    }

    interval() {
        throw new Error(`${this.constructor.name} doesn't have a ${this.interval.name}() method.`);
    }
}

module.exports = Interval;