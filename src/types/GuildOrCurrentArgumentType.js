'use strict';

const { GlobalPaths } = require('globalobjects');

const ArgumentType = require(GlobalPaths.ArgumentType);

const GuildArgumentType = require('./GuildArgumentType');

class GuildOrCurrentArgumentType extends ArgumentType {
    constructor() {
        super('guild-or-current');
        // TODO: Get it from registry?
        this.guildArgumentType = new GuildArgumentType();
    }

    isEmpty(val, message) {
        return message.guild ? false : !val;
    }

    parse(val, message, arg) {
        if (message.guild)
            return message.guild;

        return this.guildArgumentType.parse(val, message, arg);
    }
}

module.exports = GuildOrCurrentArgumentType;
