'use strict';

const Attribute = require('./Attribute.js');

class UserAttribute extends Attribute {
    constructor(options) {
        super(options);
        if (new.target === UserAttribute) {
            throw new Error(`Can't instantiate abstract ${this.constructor.name} class.`);
        }
    }

    async retrieve(commandContext, user) { // eslint-disable-line no-unused-vars
        throw new Error(`${this.constructor.name} doesn't have a retrieve() method.`);
    }

    async set(commandContext, value) { // eslint-disable-line no-unused-vars
        throw new Error(`${this.constructor.name} doesn't have a set() method.`);
    }
}

module.exports = UserAttribute;