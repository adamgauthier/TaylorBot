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

    parse(val, messageContext, arg) {
        if (messageContext.message.guild)
            return messageContext.message.guild;

        return this.guildArgumentType.parse(val, messageContext, arg);
    }
}

module.exports = GuildOrCurrentArgumentType;
