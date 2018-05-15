'use strict';

const { GlobalPaths } = require('globalobjects');

const UserGroups = require(GlobalPaths.UserGroups);

class Command {
    constructor(client, info, minimumGroup = UserGroups.Everyone) {
        if (new.target === Command) {
            throw new Error(`Can't instantiate abstract ${this.constructor.name} class.`);
        }

        this.info = info;
        this.info.separator = ' ';
        this.minimumGroup = minimumGroup;
    }
}

module.exports = Command;