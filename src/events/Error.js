'use strict';

const { Events } = require('discord.js').Constants;
const { Paths } = require('globalobjects');

const EventHandler = require(Paths.EventHandler);
const Log = require('../tools/Logger.js');

class Error extends EventHandler {
    constructor() {
        super(Events.ERROR);
    }

    handler(client, errorEvent) {
        Log.error(`Client WebSocket error encountered: ${errorEvent.error}`);
    }
}

module.exports = Error;