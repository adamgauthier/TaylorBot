'use strict';

class EventHandler {
    constructor(eventName, enabled = true) {
        if (new.target === EventHandler) {
            throw new Error(`Can't instantiate abstract ${this.constructor.name} class.`);
        }

        this.enabled = enabled;
        this.eventName = eventName;
    }

    handler() {
        throw new Error(`${this.constructor.name} doesn't have a handler() method.`);
    }
}

module.exports = EventHandler;