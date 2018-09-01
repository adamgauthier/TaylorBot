'use strict';

class Inhibitor {
    constructor() {
        if (new.target === Inhibitor) {
            throw new Error(`Can't instantiate abstract ${this.constructor.name} class.`);
        }
    }

    shouldBeBlocked() {
        throw new Error(`${this.constructor.name} doesn't have a ${this.shouldBeBlocked.name}() method.`);
    }
}

module.exports = Inhibitor;