'use strict';

class Inhibitor {
    constructor() {
        if (new.target === Inhibitor) {
            throw new Error(`Can't instantiate abstract Inhibitor class.`);
        }
    }

    shouldBeBlocked() {
        throw new Error(`${this.constructor.name} doesn't have a shouldBeBlocked() method.`);
    }
}

module.exports = Inhibitor;