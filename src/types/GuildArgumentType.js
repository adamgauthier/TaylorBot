'use strict';

const { ArgumentType } = require('discord.js-commando');

const guildFilterExact = search => {
    return guild => guild.name.toLowerCase() === search;
};

const guildFilterInexact = search => {
    return guild => guild.name.toLowerCase().includes(search);
};

class GuildArgumentType extends ArgumentType {
    constructor(client) {
        super(client, 'guild');
    }

    validate(val, msg) {
        const matches = val.match(/^([0-9]+)$/);
        if (matches) {
            const guild = msg.client.guilds.resolve(matches[1]);
            if (guild)
                return true;
        }

        const search = val.toLowerCase();
        const guilds = msg.client.guilds.filterArray(guildFilterInexact(search));
        if (guilds.length === 0)
            return false;
        if (guilds.length === 1)
            return true;

        const exactGuilds = guilds.filter(guildFilterExact(search));
        if (exactGuilds.length > 0)
            return true;

        return false;
    }

    parse(val, msg) {
        const matches = val.match(/^([0-9]+)$/);
        if (matches) {
            const guild = msg.client.guilds.resolve(matches[1]);
            if (guild)
                return guild;
        }

        const search = val.toLowerCase();
        const guilds = msg.client.guilds.filterArray(guildFilterInexact(search));
        if (guilds.length === 0)
            return null;
        if (guilds.length === 1)
            return guilds[0];

        const exactGuilds = guilds.filter(guildFilterExact(search));
        if (exactGuilds.length > 0)
            return exactGuilds[0];

        return null;
    }
}

module.exports = GuildArgumentType;
