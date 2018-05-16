'use strict';

const { GlobalPaths } = require('globalobjects');

const UserGroups = require(GlobalPaths.UserGroups);

class Command {
    constructor(info, minimumGroup = UserGroups.Everyone) {
        if (new.target === Command) {
            throw new Error(`Can't instantiate abstract ${this.constructor.name} class.`);
        }

        this.info = info;
        this.info.separator = ' ';
        if (!this.info.aliases)
            this.info.aliases = [];
        this.minimumGroup = minimumGroup;
    }

    run(commandMessage, args) { // eslint-disable-line no-unused-vars
        throw new Error(`${this.constructor.name} doesn't have a run() method.`);
    }
}

module.exports = Command;