'use strict';

class Attribute {
    constructor({ id, aliases, description, canList }) {
        if (new.target === Attribute) {
            throw new Error(`Can't instantiate abstract ${this.constructor.name} class.`);
        }

        if (!id) {
            throw new Error(`All attributes must have an id. (${this.constructor.name})`);
        }

        this.id = id;
        this.aliases = aliases === undefined ? [] : aliases;
        this.description = description;
        this.canList = canList === undefined ? false : canList;
    }

    async list(commandContext, guild) { // eslint-disable-line no-unused-vars
        throw new Error(`${this.constructor.name} doesn't have a ${this.list.name}() method.`);
    }
}

module.exports = Attribute;