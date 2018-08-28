'use strict';

const ArgumentType = require('../structures/ArgumentType.js');

class MultilineTextArgumentType extends ArgumentType {
    constructor() {
        super({
            includesSpaces: true,
            includesNewLines: true
        });
    }

    get id() {
        return 'multiline-text';
    }

    parse(val) {
        return val;
    }
}

module.exports = MultilineTextArgumentType;