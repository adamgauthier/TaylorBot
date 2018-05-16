'use strict';

const { GlobalPaths } = require('globalobjects');

const ArgumentType = require(GlobalPaths.ArgumentType);

class UserGroupArgumentType extends ArgumentType {
    constructor() {
        super('user-group');
    }

    parse(val) {
        for (const name of this.client.oldRegistry.groups.keys()) {
            if (name.toLowerCase() === val.toLowerCase())
                return this.client.oldRegistry.groups.get(name);
        }
        return false;
    }
}

module.exports = UserGroupArgumentType;