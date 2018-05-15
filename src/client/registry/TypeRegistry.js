'use strict';

const { GlobalPaths } = require('globalobjects');

const TypeLoader = require(GlobalPaths.TypeLoader);

class TypeRegistry extends Map {
    async loadAll() {
        const types = await TypeLoader.loadAll();

        types.forEach(t => this.cacheType(t));
    }

    cacheType(type) {
        if (this.has(type.id)) {
            throw new Error(`Can't cache '${type.id}' because this id is already cached.`);
        }

        this.set(type.id, type);
    }

    getType(typeId) {
        const type = this.get(typeId);

        if (!type) {
            throw new Error(`Type '${typeId}' is not cached.`);
        }

        return type;
    }
}

module.exports = TypeRegistry;