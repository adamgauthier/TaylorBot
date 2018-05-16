'use strict';

class ArgumentType {
    constructor(id) {
        if (new.target === ArgumentType) {
            throw new Error(`Can't instantiate abstract ${this.constructor.name} class.`);
        }

        if (id === undefined) {
            throw new Error(`An ArgumentType must have an id.`);
        }

        this.id = id;
    }

    isEmpty(val, message, arg) { // eslint-disable-line no-unused-vars
        return !val;
    }

    parse(val, message, arg) { // eslint-disable-line no-unused-vars
        throw new Error(`${this.constructor.name} doesn't have a parse() method.`);
    }
}

module.exports = ArgumentType;
