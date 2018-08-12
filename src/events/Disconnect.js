'use strict';

const { Events } = require('discord.js').Constants;
const { Paths } = require('globalobjects');

const EventHandler = require(Paths.EventHandler);
const Log = require('../tools/Logger.js');

class Disconnect extends EventHandler {
    constructor() {
        super(Events.DISCONNECT);
    }

    handler(taylorbot, closeEvent) {
        Log.info(`Client was disconnected! Reason: ${closeEvent.code} - ${closeEvent.reason}`);

        taylorbot.intervalRunner.stopAll();
        Log.info('Intervals stopped!');
    }
}

module.exports = Disconnect;