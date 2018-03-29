'use strict';

const { GlobalPaths } = require('globalobjects');

const Log = require(GlobalPaths.Logger);

class Registry {
    constructor(client) {
        this.client = client;

        this.types = new TypeRegistry();
    }

    async loadAll() {
        Log.info('Loading type registry...');
        await this.registry.types.loadAll(this.client);
        Log.info('Type registry loaded!');
    }
}

module.exports = Registry;