'use strict';

const { GlobalPaths } = require('globalobjects');

const EventHandler = require(GlobalPaths.EventHandler);

class Message extends EventHandler {
    constructor() {
        super((taylorbot, message) => {
            taylorbot.watcherFeeder.feedAll(message);
        });
    }
}

module.exports = new Message();