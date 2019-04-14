'use strict';

const { Events } = require('discord.js').Constants;

const EventHandler = require('../structures/EventHandler.js');
const Log = require('../tools/Logger.js');

class ShardDisconnected extends EventHandler {
    constructor() {
        super(Events.SHARD_DISCONNECTED);
    }

    handler(taylorbot, closeEvent, number) {
        Log.info(`Shard ${number} disconnected! Reason: ${closeEvent.code} - ${closeEvent.reason}`);

        taylorbot.intervalRunner.stopAll();
        Log.info('Intervals stopped!');
    }
}

module.exports = ShardDisconnected;