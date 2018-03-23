'use strict';

const { GlobalPaths } = require('globalobjects');

const AccessLevels = require(GlobalPaths.AccessLevels);

class Command {
    constructor(handler, alternateNames = [], accessLevel = AccessLevels.EVERYONE) {
        if (new.target === Command) {
            throw new Error(`Can't instantiate abstract Command class.`);
        }

        this.handler = handler;
        this.alternateNames = alternateNames;
        this.accessLevel = accessLevel;
    }
}

module.exports = Command;