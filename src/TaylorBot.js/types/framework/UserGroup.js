'use strict';

const WordArgumentType = require('../base/Word.js');
const ArgumentParsingError = require('../ArgumentParsingError.js');

class UserGroupArgumentType extends WordArgumentType {
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