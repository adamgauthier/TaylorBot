'use strict';

const { GlobalPaths } = require('globalobjects');

const TypeLoader = require(GlobalPaths.TypeLoader);

class TypeSettings extends Map {
    constructor() {
        super();
    }

    async loadAll(taylorbot) {
        const types = await TypeLoader.loadAll(taylorbot);

        for (const type of types) {
            this.registerType(type.id, type);
        }
    }

    registerType(id, type) {
        if (this.has(id))
            throw new Error(`Registering type ${id}, was already registered.`);

        this.set(id, type);
    }
}

module.exports = TypeSettings;