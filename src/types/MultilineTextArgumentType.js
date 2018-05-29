'use strict';

const { Paths } = require('globalobjects');

const ArgumentType = require(Paths.ArgumentType);

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