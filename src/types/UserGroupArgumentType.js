'use strict';

const { Paths } = require('globalobjects');

const ArgumentType = require(Paths.ArgumentType);
const ArgumentParsingError = require(Paths.ArgumentParsingError);

class UserGroupArgumentType extends ArgumentType {
    get id() {
        return 'user-group';
    }

    parse(val, { client }) {
        for (const group of client.master.registry.groups.values()) {
            if (group.name.toLowerCase() === val.toLowerCase())
                return group;
        }

        throw new ArgumentParsingError(`User Group '${val.toLowerCase()}' doesn't exist.`);
    }
}

module.exports = UserGroupArgumentType;