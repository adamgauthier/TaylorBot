'use strict';

const { ArgumentType } = require('discord.js-commando');

class UserGroupArgumentType extends ArgumentType {
    constructor(client) {
        super(client, 'user-group');
    }

    validate(val) {
        for (const name of this.client.oldRegistry.groups.keys()) {
            if (name.toLowerCase() === val.toLowerCase())
                return true;
        }
        return false;
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