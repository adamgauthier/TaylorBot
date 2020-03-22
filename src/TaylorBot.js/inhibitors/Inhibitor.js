'use strict';

class Inhibitor {
    constructor() {
        if (new.target === Inhibitor) {
            throw new Error(`Can't instantiate abstract ${this.constructor.name} class.`);
        }
    }
}

module.exports = Inhibitor;