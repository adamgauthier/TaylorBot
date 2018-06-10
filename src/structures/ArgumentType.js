'use strict';

class ArgumentType {
    constructor({ includesSpaces, includesNewLines, mustBeQuoted } = {}) {
        if (new.target === ArgumentType) {
            throw new Error(`Can't instantiate abstract ${this.constructor.name} class.`);
        }

        this.includesSpaces = includesSpaces === undefined ? false : includesSpaces;
        this.includesNewLines = includesNewLines === undefined ? false : includesNewLines;
        this.mustBeQuoted = mustBeQuoted === undefined ? false : mustBeQuoted;
    }

    get id() {
        throw new Error(`${this.constructor.name} must have an id.`);
    }

    canBeEmpty(messageContext, arg) { // eslint-disable-line no-unused-vars
        return false;
    }

    default(commandContext, arg) { // eslint-disable-line no-unused-vars
        throw new Error(`${this.constructor.name} doesn't have a default.`);
    }

    parse(val, commandContext, arg) { // eslint-disable-line no-unused-vars
        throw new Error(`${this.constructor.name} doesn't have a parse() method.`);
    }
}

module.exports = ArgumentType;
