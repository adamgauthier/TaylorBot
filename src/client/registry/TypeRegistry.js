'use strict';

const { GlobalPaths } = require('globalobjects');

const TypeLoader = require(GlobalPaths.TypeLoader);

class TypeRegistry {
    async loadAll(client) {
        const types = await TypeLoader.loadAll();

        client.registry.registerTypes(types);
    }
}

module.exports = TypeRegistry;