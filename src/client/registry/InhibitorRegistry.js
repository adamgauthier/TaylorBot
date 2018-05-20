'use strict';

const { Paths } = require('globalobjects');

const InhibitorLoader = require(Paths.InhibitorLoader);

class InhibitorRegistry extends Set {
    async loadAll() {
        const inhibitors = await InhibitorLoader.loadAll();

        inhibitors.forEach(i => this.add(i));
    }
}

module.exports = InhibitorRegistry;