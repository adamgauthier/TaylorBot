'use strict';

class ArgumentType {
    constructor(id, { includesSpaces, includesNewLines } = {}) {
        if (new.target === ArgumentType) {
            throw new Error(`Can't instantiate abstract ${this.constructor.name} class.`);
        }

        if (id === undefined) {
            throw new Error(`An ArgumentType must have an id.`);
        }

        this.id = id;
        this.includesSpaces = includesSpaces === undefined ? false : includesSpaces;
        this.includesNewLines = includesNewLines === undefined ? false : includesNewLines;
    }

    canBeEmpty(messageContext, arg) { // eslint-disable-line no-unused-vars
        return false;
    }

    isEmpty(val, messageContext, arg) { // eslint-disable-line no-unused-vars
        return !val;
    }

    parse(val, messageContext, arg) { // eslint-disable-line no-unused-vars
        throw new Error(`${this.constructor.name} doesn't have a parse() method.`);
    }
}

module.exports = ArgumentType;
