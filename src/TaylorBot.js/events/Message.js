'use strict';

const { Events } = require('discord.js').Constants;

const EventHandler = require('../structures/EventHandler.js');

class Message extends EventHandler {
    constructor() {
        super(Events.MESSAGE_CREATE);
    }

    handler(client, message) {
        client.master.registry.watchers.feedAll(client, message);
    }
}

module.exports = Message;