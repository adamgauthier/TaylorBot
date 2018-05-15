'use strict';

const { GlobalPaths } = require('globalobjects');

const ArgumentType = require(GlobalPaths.ArgumentType);

const GuildArgumentType = require('./GuildArgumentType');

class GuildOrCurrentArgumentType extends ArgumentType {
    constructor(client) {
        super(client, 'guild-or-current');
        this.guildArgumentType = new GuildArgumentType(client);
    }

    get id() {
        return 'guild-or-current';
    }

    isEmpty(val, msg) {
        return msg.guild ? false : !val;
    }

    validate(val, msg, arg) {
        if (msg.guild)
            return true;

        return this.guildArgumentType.validate(val, msg, arg);
    }

    parse(val, msg, arg) {
        if (msg.guild)
            return msg.guild;

        return this.guildArgumentType.parse(val, msg, arg);
    }
}

module.exports = GuildOrCurrentArgumentType;
