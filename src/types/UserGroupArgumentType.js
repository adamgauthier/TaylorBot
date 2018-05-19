'use strict';

const { GlobalPaths } = require('globalobjects');

const ArgumentType = require(GlobalPaths.ArgumentType);
const ArgumentParsingError = require(GlobalPaths.ArgumentParsingError);

class UserGroupArgumentType extends ArgumentType {
    constructor() {
        super('user-group');
    }

    parse(val, message) {
        for (const group of message.client.oldRegistry.groups.values()) {
            if (group.name.toLowerCase() === val.toLowerCase())
                return group;
        }

        throw new ArgumentParsingError(`User Group '${val.toLowerCase()}' doesn't exist.`);
    }
}

module.exports = UserGroupArgumentType;