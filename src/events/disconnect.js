'use strict';

const { GlobalPaths } = require('globalobjects');

const EventHandler = require(GlobalPaths.EventHandler);
const Log = require(GlobalPaths.Logger);

class Disconnect extends EventHandler {
    constructor() {
        super();
    }

    handler(taylorbot, closeEvent) {
        Log.info(`Client was disconnected! Reason: ${closeEvent.code} - ${closeEvent.reason}`);

        taylorbot.intervalRunner.stopAll();
        Log.info('Intervals stopped!');
    }
}

module.exports = Disconnect;