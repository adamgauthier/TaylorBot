'use strict';

const ArgumentType = require('../ArgumentType.js');

class TextArgumentType extends ArgumentType {
    constructor() {
        super({
            includesSpaces: true
        });
    }

    get id() {
        return 'text';
    }

    parse(val) {
        return val;
    }
}

module.exports = TextArgumentType;