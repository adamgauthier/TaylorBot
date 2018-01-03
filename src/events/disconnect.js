'use strict';

const path = require('path');
const GlobalPaths = require(path.join(__dirname, '..', 'GlobalPaths'));

const EventHandler = require(GlobalPaths.EventHandler);
const taylorbot = require(GlobalPaths.taylorBotClient);
const Log = require(GlobalPaths.Logger);

class Disconnect extends EventHandler {
    constructor() {
        super(closeEvent => {
            Log.info(`Client was disconnected! Reason: ${closeEvent.code} - ${closeEvent.reason}`);

            taylorbot.intervalRunner.stopAll();
            Log.info('Intervals stopped!');
        });
    }
}

module.exports = new Disconnect();