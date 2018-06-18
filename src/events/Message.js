'use strict';

const { Events } = require('discord.js').Constants;
const { Paths } = require('globalobjects');

const EventHandler = require(Paths.EventHandler);

class Message extends EventHandler {
    constructor() {
        super(Events.MESSAGE_CREATE);
    }

    handler(client, message) {
        client.master.registry.watchers.feedAll(client, message);
    }
}

module.exports = Message;