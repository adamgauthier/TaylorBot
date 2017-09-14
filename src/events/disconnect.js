'use strict';

const path = require('path');
const GlobalPaths = require(path.join(__dirname, '..', 'GlobalPaths'));

const EventHandler = require(GlobalPaths.EventHandler);
const intervalRunner = require(GlobalPaths.intervalRunner);

class Disconnect extends EventHandler {
    constructor() {
        super(closeEvent => {
            console.log(`Client was disconnected! Reason: ${closeEvent.code} - ${closeEvent.reason}`);

            intervalRunner.stopAll();
            console.log('Intervals stopped!');
        });
    }
}

module.exports = new Disconnect();