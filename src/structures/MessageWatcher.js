'use strict';

class MessageWatcher {
    constructor(messageHandler, enabled = true) {
        if (new.target === MessageWatcher) {
            throw new Error(`Can't instantiate abstract MessageWatcher class.`);
        }

        this.enabled = enabled;
    }

    messageHandler() {
        throw new Error(`${this.constructor.name} doesn't have a handler() method.`);
    }
}

module.exports = MessageWatcher;