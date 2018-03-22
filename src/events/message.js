'use strict';

const { GlobalPaths } = require('globalobjects');

const EventHandler = require(GlobalPaths.EventHandler);
const taylorbot = require(GlobalPaths.taylorBotClient);

class Message extends EventHandler {
    constructor() {
        super(message => {
            taylorbot.watcherFeeder.feedAll(message);
        });
    }
}

module.exports = new Message();