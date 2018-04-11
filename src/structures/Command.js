'use strict';

const Commando = require('discord.js-commando');
const { GlobalPaths } = require('globalobjects');

const UserGroups = require(GlobalPaths.UserGroups);

class Command extends Commando.Command {
    constructor(client, info, minimumGroup = UserGroups.Everyone) {
        super(client, info);
        if (new.target === Command) {
            throw new Error(`Can't instantiate abstract ${this.constructor.name} class.`);
        }

        this.minimumGroup = minimumGroup;
    }
}

module.exports = Command;