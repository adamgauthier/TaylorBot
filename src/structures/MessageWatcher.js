'use strict';

class MessageWatcher {
    constructor(messageHandler, enabled = true) {
        if (new.target === MessageWatcher) {
            throw new Error(`Can't instantiate abstract MessageWatcher class.`);
        }

        this.messageHandler = messageHandler;
        this.enabled = enabled;
    }
}

module.exports = MessageWatcher;