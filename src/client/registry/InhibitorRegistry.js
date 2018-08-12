'use strict';

const InhibitorLoader = require('../../modules/InhibitorLoader.js');

class InhibitorRegistry extends Set {
    async loadAll() {
        const inhibitors = await InhibitorLoader.loadAll();

        inhibitors.forEach(i => this.add(i));
    }
}

module.exports = InhibitorRegistry;