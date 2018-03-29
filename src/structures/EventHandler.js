'use strict';

class EventHandler {
    constructor(enabled = true) {
        if (new.target === EventHandler) {
            throw new Error(`Can't instantiate abstract EventHandler class.`);
        }

        this.enabled = enabled;
    }

    handler() {
        throw new Error(`${this.constructor.name} doesn't have a handler() method.`);
    }
}

module.exports = EventHandler;