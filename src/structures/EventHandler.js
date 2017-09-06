'use strict';

class EventHandler {
    constructor(handler, enabled = true) {
        if (new.target === EventHandler) {
            throw new Error(`Can't instantiate abstract EventHandler class.`);
        }

        this.handler = handler;
        this.enabled = enabled;
    }
}

module.exports = EventHandler;