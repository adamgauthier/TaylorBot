'use strict';

const { GlobalPaths } = require('globalobjects');

const ArgumentType = require(GlobalPaths.ArgumentType);
const ArgumentParsingError = require(GlobalPaths.ArgumentParsingError);

class GuildArgumentType extends ArgumentType {
    constructor() {
        super('guild');
    }

    async parse(val, message) {
        const matches = val.match(/^([0-9]+)$/);
        if (matches) {
            const guild = message.client.guilds.resolve(matches[1]);
            if (guild) {
                const member = await guild.members.fetch(message.author);
                if (member) {
                    return guild;
                }
            }
        }

        const search = val.toLowerCase();
        const guilds = message.client.guilds.filterArray(GuildArgumentType.guildFilterInexact(search));
        if (guilds.length === 0) {
            throw new ArgumentParsingError(`Could not find server '${val}'.`);
        }
        else if (guilds.length === 1) {
            const guild = guilds[0];
            const member = await guild.members.fetch(message.author);
            if (member) {
                return guild;
            }
            else {
                throw new ArgumentParsingError(`Could not find server '${val}' that you are a part of.`);
            }
        }

        const exactGuilds = guilds.filter(GuildArgumentType.guildFilterExact(search));

        for (const guild of exactGuilds) {
            const member = await guild.members.fetch(message.author);
            if (member)
                return guild;
        }

        for (const guild of guilds) {
            const member = await guild.members.fetch(message.author);
            if (member)
                return guild;
        }

        throw new ArgumentParsingError(`Could not find server '${val}' that you are a part of.`);
    }

    static guildFilterExact(search) {
        return guild => guild.name.toLowerCase() === search;
    }

    static guildFilterInexact(search) {
        return guild => guild.name.toLowerCase().includes(search);
    }
}

module.exports = GuildArgumentType;
