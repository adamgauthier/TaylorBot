'use strict';

const ArgumentType = require('../ArgumentType.js');

class WordArgumentType extends ArgumentType {
    get id() {
        return 'word';
    }

    parse(val) {
        return val;
    }
}

module.exports = WordArgumentType;