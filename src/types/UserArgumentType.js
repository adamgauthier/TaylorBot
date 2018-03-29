'use strict';

const { GlobalPaths } = require('globalobjects');

const ArgumentType = require(GlobalPaths.ArgumentType);

class UserArgumentType extends ArgumentType {
    constructor(taylorbot) {
        super(taylorbot, 'user');
    }

    validate(val, arg, message) {
        // TODO: actually validate
        return false;
    }

    parse(val, arg, message) {
        // TODO: actually parse
        return val;
    }
}

module.exports = UserArgumentType;