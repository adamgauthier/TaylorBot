'use strict';

const { Paths } = require('globalobjects');

const ArgumentType = require(Paths.ArgumentType);

class QuotedMultilineTextArgumentType extends ArgumentType {
    constructor() {
        super({
            includesSpaces: true,
            includesNewLines: true,
            mustBeQuoted: true
        });
    }

    get id() {
        return 'quoted-multiline-text';
    }

    parse(val) {
        return val;
    }
}

module.exports = QuotedMultilineTextArgumentType;