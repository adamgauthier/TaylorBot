'use strict';

const { Paths } = require('globalobjects');

const EventHandler = require(Paths.EventHandler);
const Log = require(Paths.Logger);

class Disconnect extends EventHandler {
    constructor() {
        super('disconnect');
    }

    handler(taylorbot, closeEvent) {
        Log.info(`Client was disconnected! Reason: ${closeEvent.code} - ${closeEvent.reason}`);

        taylorbot.intervalRunner.stopAll();
        Log.info('Intervals stopped!');
    }
}

module.exports = Disconnect;