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

    async validate(val, msg) {
        const matches = val.match(/^([0-9]+)$/);
        if (matches) {
            const guild = msg.client.guilds.resolve(matches[1]);
            if (guild) {
                const member = await guild.members.fetch(msg.author);
                if (member) {
                    return true;
                }
            }
        }

        const search = val.toLowerCase();
        const guilds = msg.client.guilds.filterArray(guildFilterInexact(search));
        if (guilds.length === 0)
            return false;
        if (guilds.length === 1) {
            const guild = guilds[0];
            const member = await guild.members.fetch(msg.author);
            return member ? true : false;
        }

        const exactGuilds = guilds.filter(guildFilterExact(search));
        if (exactGuilds.length > 0) {
            for (const guild of exactGuilds) {
                const member = await guild.members.fetch(msg.author);
                if (member)
                    return true;
            }
        }

        return false;
    }

    async parse(val, msg) {
        const matches = val.match(/^([0-9]+)$/);
        if (matches) {
            const guild = msg.client.guilds.resolve(matches[1]);
            if (guild) {
                const member = await guild.members.fetch(msg.author);
                if (member) {
                    return guild;
                }
            }
        }

        const search = val.toLowerCase();
        const guilds = msg.client.guilds.filterArray(guildFilterInexact(search));
        if (guilds.length === 0)
            return null;
        if (guilds.length === 1) {
            const guild = guilds[0];
            const member = await guild.members.fetch(msg.author);
            return member ? guild : null;
        }

        const exactGuilds = guilds.filter(guildFilterExact(search));
        if (exactGuilds.length > 0) {
            for (const guild of exactGuilds) {
                const member = await guild.members.fetch(msg.author);
                if (member)
                    return guild;
            }
        }

        return null;
    }
}

module.exports = GuildArgumentType;
