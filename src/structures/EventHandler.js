'use strict';

class EventHandler {
    constructor(eventName, enabled = true) {
        if (new.target === EventHandler) {
            throw new Error(`Can't instantiate abstract ${this.constructor.name} class.`);
        }

        if (!eventName) {
            throw new Error(`All event handlers must have an eventName. (${this.constructor.name})`);
        }

        this.enabled = enabled;
        this.eventName = eventName;
    }

    handler() {
        throw new Error(`${this.constructor.name} doesn't have a ${this.handler.name}() method.`);
    }
}

module.exports = EventHandler;