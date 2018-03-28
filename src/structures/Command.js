'use strict';

const { GlobalPaths } = require('globalobjects');

const DefaultGroups = require(GlobalPaths.DefaultGroups);

class Command {
    constructor(handler, aliases = [], minimumGroup = DefaultGroups.Everyone) {
        if (new.target === Command) {
            throw new Error(`Can't instantiate abstract Command class.`);
        }

        this.handler = handler;
        this.aliases = aliases;
        this.minimumGroup = minimumGroup;
    }
}

module.exports = Command;