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

    isEmpty(val, msg) {
        return msg.guild ? false : !val;
    }

    parse(val, msg, arg) {
        if (msg.guild)
            return msg.guild;

        return this.guildArgumentType.parse(val, msg, arg);
    }
}

module.exports = GuildOrCurrentArgumentType;
