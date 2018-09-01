'use strict';

class Loggable {
    toEmbed() {
        throw new Error(`${this.constructor.name} doesn't have a ${this.toEmbed.name}() method.`);
    }
}

module.exports = Loggable;