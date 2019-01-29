'use strict';

const GuildArgumentType = require('./Guild.js');

class GuildOrCurrentArgumentType extends GuildArgumentType {
    get id() {
        return 'guild-or-current';
    }

    canBeEmpty({ message }) {
        return message.guild ? true : false;
    }

    default({ message }) {
        return message.guild;
    }
}

module.exports = GuildOrCurrentArgumentType;
