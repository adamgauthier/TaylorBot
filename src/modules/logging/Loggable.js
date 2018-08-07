'use strict';

class Loggable {
    toEmbed() {
        throw new Error(`${this.constructor.name} doesn't have a toEmbed() method.`);
    }
}

module.exports = Loggable;