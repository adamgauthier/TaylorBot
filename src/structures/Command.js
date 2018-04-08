'use strict';

const Commando = require('discord.js-commando');
const { GlobalPaths } = require('globalobjects');

const UserGroups = require(GlobalPaths.UserGroups);

class Command extends Commando.Command {
    constructor(client, info, minimumGroup = UserGroups.Everyone) {
        if (new.target === Command) {
            throw new Error(`Can't instantiate abstract Command class.`);
        }
        super(client, info);

        this.minimumGroup = minimumGroup;
    }
}

module.exports = Command;