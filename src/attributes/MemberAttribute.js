'use strict';

class MemberAttribute {
    constructor({ id, description }) {
        if (new.target === MemberAttribute) {
            throw new Error(`Can't instantiate abstract ${this.constructor.name} class.`);
        }

        if (!id) {
            throw new Error(`All attributes must have an id. (${this.constructor.name})`);
        }

        this.id = id;
        this.description = description;
    }

    async retrieve(commandContext, member) { // eslint-disable-line no-unused-vars
        throw new Error(`${this.constructor.name} doesn't have a retrieve() method.`);
    }

    async rank(commandContext, guild) { // eslint-disable-line no-unused-vars
        throw new Error(`${this.constructor.name} doesn't have a rank() method.`);
    }
}

module.exports = MemberAttribute;