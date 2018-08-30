'use strict';

class Attribute {
    constructor({ id, description }) {
        if (new.target === Attribute) {
            throw new Error(`Can't instantiate abstract ${this.constructor.name} class.`);
        }

        if (!id) {
            throw new Error(`All attributes must have an id. (${this.constructor.name})`);
        }

        this.id = id;
        this.description = description;
    }
}

module.exports = Attribute;