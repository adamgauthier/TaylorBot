'use strict';

const AttributeLoader = require('../../attributes/AttributeLoader.js');

class AttributeRegistry extends Map {
    async loadAll() {
        const attributes = await AttributeLoader.loadMemberAttributes();

        attributes.forEach(a => this.set(a.id, a));
    }
}

module.exports = AttributeRegistry;