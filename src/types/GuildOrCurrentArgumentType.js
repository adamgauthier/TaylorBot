'use strict';

const { Paths } = require('globalobjects');

const ArgumentType = require(Paths.ArgumentType);

const GuildArgumentType = require('./GuildArgumentType');

class GuildOrCurrentArgumentType extends ArgumentType {
    constructor() {
        super('guild-or-current');
        // TODO: Get it from registry?
        this.guildArgumentType = new GuildArgumentType();
    }

    canBeEmpty({ message }) {
        return message.guild ? true : false;
    }

    parse(val, message, arg) {
        if (message.guild)
            return message.guild;

        return this.guildArgumentType.parse(val, message, arg);
    }
}

module.exports = GuildOrCurrentArgumentType;
