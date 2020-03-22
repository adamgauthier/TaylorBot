'use strict';

const Inhibitor = require('./Inhibitor.js');

class SilentInhibitor extends Inhibitor {
    constructor() {
        super();
        if (new.target === SilentInhibitor) {
            throw new Error(`Can't instantiate abstract ${this.constructor.name} class.`);
        }
    }

    shouldBeBlocked(messageContext, cachedCommand, argString) { // eslint-disable-line no-unused-vars
        throw new Error(`${this.constructor.name} doesn't have a ${this.shouldBeBlocked.name}() method.`);
    }
}

module.exports = SilentInhibitor;