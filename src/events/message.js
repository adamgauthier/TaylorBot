'use strict';

const { GlobalPaths } = require('globalobjects');

const EventHandler = require(GlobalPaths.EventHandler);

class Message extends EventHandler {
    constructor() {
        super();
    }

    handler(taylorbot, message) {
        taylorbot.oldRegistry.watchers.feedAll(taylorbot, message);
    }
}

module.exports = Message;