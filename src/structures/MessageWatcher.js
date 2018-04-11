'use strict';

class MessageWatcher {
    constructor(enabled = true) {
        if (new.target === MessageWatcher) {
            throw new Error(`Can't instantiate abstract ${this.constructor.name} class.`);
        }

        this.enabled = enabled;
    }

    messageHandler() {
        throw new Error(`${this.constructor.name} doesn't have a messageHandler() method.`);
    }
}

module.exports = MessageWatcher;