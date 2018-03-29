'use strict';

const { GlobalPaths } = require('globalobjects');

const Log = require(GlobalPaths.Logger);
const TypeRegistry = require(GlobalPaths.TypeRegistry);
const MessageWatcherRegistry = require(GlobalPaths.MessageWatcherRegistry);

class Registry {
    constructor(client) {
        this.client = client;

        this.types = new TypeRegistry();
        this.watchers = new MessageWatcherRegistry();
    }

    async loadAll() {
        Log.info('Loading type registry...');
        await this.types.loadAll(this.client);
        Log.info('Type registry loaded!');

        Log.info('Loading message watchers...');
        this.watchers.loadAll();
        Log.info('Message watchers loaded!');
    }
}

module.exports = Registry;