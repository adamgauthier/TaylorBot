'use strict';

const Inhibitor = require('./Inhibitor.js');

class NoisyInhibitor extends Inhibitor {
    constructor() {
        super();
        if (new.target === NoisyInhibitor) {
            throw new Error(`Can't instantiate abstract ${this.constructor.name} class.`);
        }
    }

    getBlockedMessage(messageContext, cachedCommand, argString) { // eslint-disable-line no-unused-vars
        throw new Error(`${this.constructor.name} doesn't have a ${this.getBlockedMessage.name}() method.`);
    }
}

module.exports = NoisyInhibitor;