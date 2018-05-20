'use strict';

const { Paths } = require('globalobjects');

const ArgumentType = require(Paths.ArgumentType);
const ArgumentParsingError = require(Paths.ArgumentParsingError);

class UserGroupArgumentType extends ArgumentType {
    constructor() {
        super('user-group');
    }

    parse(val, message) {
        for (const group of message.client.master.oldRegistry.groups.values()) {
            if (group.name.toLowerCase() === val.toLowerCase())
                return group;
        }

        throw new ArgumentParsingError(`User Group '${val.toLowerCase()}' doesn't exist.`);
    }
}

module.exports = UserGroupArgumentType;