'use strict';

const Attribute = require('./Attribute.js');

class MemberAttribute extends Attribute {
    constructor(options) {
        super(options);
        if (new.target === MemberAttribute) {
            throw new Error(`Can't instantiate abstract ${this.constructor.name} class.`);
        }
    }

    async getCommand(commandContext, member) { // eslint-disable-line no-unused-vars
        throw new Error(`${this.constructor.name} doesn't have a ${this.getCommand.name}() method.`);
    }

    async rankCommand(commandContext, guild) { // eslint-disable-line no-unused-vars
        throw new Error(`${this.constructor.name} doesn't have a ${this.rank.name}() method.`);
    }
}

module.exports = MemberAttribute;