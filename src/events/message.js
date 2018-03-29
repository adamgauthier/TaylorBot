'use strict';

const { GlobalPaths } = require('globalobjects');

const EventHandler = require(GlobalPaths.EventHandler);

class Message extends EventHandler {
    constructor() {
        super();
    }

    handler(taylorbot, message) {
        taylorbot.registry.watchers.feedAll(taylorbot, message);
    }
}

module.exports = new Message();