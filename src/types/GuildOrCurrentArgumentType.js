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

    parse(val, commandContext, arg) {
        if (commandContext.message.guild)
            return commandContext.message.guild;

        return this.guildArgumentType.parse(val, commandContext, arg);
    }
}

module.exports = GuildOrCurrentArgumentType;
