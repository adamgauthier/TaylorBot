'use strict';

const { GlobalPaths } = require('globalobjects');

const ArgumentType = require(GlobalPaths.ArgumentType);

class UserArgumentType extends ArgumentType {
    constructor(taylorbot) {
        super(taylorbot, 'user');
    }

    validate(val, message, arg) {
        // TODO: actually validate
        return false;
    }

    parse(val, message, arg) {
        // TODO: actually parse
        return message.author;
    }
}

module.exports = UserArgumentType;