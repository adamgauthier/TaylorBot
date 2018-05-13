'use strict';

const { GlobalPaths } = require('globalobjects');

const InhibitorLoader = require(GlobalPaths.InhibitorLoader);

class InhibitorRegistry extends Set {
    async loadAll() {
        const inhibitors = await InhibitorLoader.loadAll();

        inhibitors.forEach(i => this.add(i));
    }
}

module.exports = InhibitorRegistry;