'use strict';

const { GlobalPaths } = require('globalobjects');

class ArgumentType {
    constructor(taylorbot, id) {
        if (new.target === ArgumentType) {
            throw new Error(`Can't instantiate abstract ArgumentType class.`);
        }

        this.taylorbot = taylorbot;
        this.id = id;
    }

    validate(val, arg, message) {
        throw new Error(`${this.constructor.name} doesn't have a validate() method.`);
    }

    parse(val, arg, message) {
        throw new Error(`${this.constructor.name} doesn't have a parse() method.`);
    }
}

module.exports = ArgumentType;