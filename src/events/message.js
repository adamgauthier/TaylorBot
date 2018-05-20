'use strict';

const { Paths } = require('globalobjects');

const EventHandler = require(Paths.EventHandler);

class Message extends EventHandler {
    constructor() {
        super();
    }

    handler(taylorbot, message) {
        taylorbot.oldRegistry.watchers.feedAll(taylorbot, message);
    }
}

module.exports = Message;