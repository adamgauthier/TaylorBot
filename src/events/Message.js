'use strict';

const { Paths } = require('globalobjects');

const EventHandler = require(Paths.EventHandler);

class Message extends EventHandler {
    constructor() {
        super('message');
    }

    handler(client, message) {
        client.master.registry.watchers.feedAll(client, message);
    }
}

module.exports = Message;