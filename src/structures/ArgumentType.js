'use strict';

class ArgumentType {
    constructor() {
        if (new.target === ArgumentType) {
            throw new Error(`Can't instantiate abstract ${this.constructor.name} class.`);
        }
    }

    get id() {
        throw new Error(`${this.constructor.name} doesn't have a id() method.`);
    }

    isEmpty(val, msg, arg) { // eslint-disable-line no-unused-vars
        return !val;
    }

    parse(val, msg, arg) { // eslint-disable-line no-unused-vars
        throw new Error(`${this.constructor.name} doesn't have a parse() method.`);
    }
}

module.exports = ArgumentType;
