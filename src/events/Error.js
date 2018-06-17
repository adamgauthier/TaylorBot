'use strict';

const { Paths } = require('globalobjects');

const EventHandler = require(Paths.EventHandler);
const Log = require(Paths.Logger);

class Error extends EventHandler {
    constructor() {
        super('error');
    }

    handler(client, errorEvent) {
        Log.error(`Client WebSocket error encountered: ${errorEvent.error}`);
    }
}

module.exports = Error;