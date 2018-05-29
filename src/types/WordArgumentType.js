'use strict';

const { Paths } = require('globalobjects');

const ArgumentType = require(Paths.ArgumentType);

class WordArgumentType extends ArgumentType {
    get id() {
        return 'word';
    }

    parse(val) {
        return val;
    }
}

module.exports = WordArgumentType;