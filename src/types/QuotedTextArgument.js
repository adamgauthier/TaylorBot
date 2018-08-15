'use strict';

const ArgumentType = require('../structures/ArgumentType.js');

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