'use strict';

const { Paths } = require('globalobjects');

const ArgumentType = require(Paths.ArgumentType);

class QuotedTextArgumentType extends ArgumentType {
    constructor() {
        super({
            includesSpaces: true,
            mustBeQuoted: true
        });
    }

    get id() {
        return 'quoted-text';
    }

    parse(val) {
        return val;
    }
}

module.exports = QuotedTextArgumentType;