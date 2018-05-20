'use strict';

const { GlobalPaths } = require('globalobjects');

const EventHandler = require(GlobalPaths.EventHandler);
const Log = require(GlobalPaths.Logger);

class Error extends EventHandler {
    handler(client, errorEvent) {
        Log.error(`Client WebSocket error encountered: ${errorEvent.error}`);
    }
}

module.exports = Error;