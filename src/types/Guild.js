'use strict';

const TextArgumentType = require('./Text.js');
const ArgumentParsingError = require('../structures/ArgumentParsingError.js');
const Log = require('../tools/Logger.js');
const Format = require('../modules/DiscordFormatter.js');

class GuildArgumentType extends TextArgumentType {
    get id() {
        return 'guild';
    }

    async parse(val, { message, client }) {
        const matches = val.trim().match(/^([0-9]+)$/);
        if (matches) {
            const guild = client.guilds.resolve(matches[1]);
            if (guild) {
                const isInGuild = await GuildArgumentType.isInGuild(message.author, guild);
                if (isInGuild)
                    return guild;
            }
        }

        const search = val.toLowerCase();
        const guilds = client.guilds.filter(GuildArgumentType.guildFilterInexact(search));
        if (guilds.size === 0) {
            throw new ArgumentParsingError(`Could not find server '${val}'.`);
        }
        else if (guilds.size === 1) {
            const guild = guilds.first();
            const isInGuild = await GuildArgumentType.isInGuild(message.author, guild);
            if (isInGuild) {
                return guild;
            }
            else {
                throw new ArgumentParsingError(`Could not find server '${val}' that you are a part of.`);
            }
        }

        const exactGuilds = guilds.filter(GuildArgumentType.guildFilterExact(search));

        for (const guild of exactGuilds.values()) {
            const isInGuild = await GuildArgumentType.isInGuild(message.author, guild);
            if (isInGuild)
                return guild;
        }

        for (const guild of guilds.values()) {
            const isInGuild = await GuildArgumentType.isInGuild(message.author, guild);
            if (isInGuild)
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

    static async isInGuild(user, guild) {
        if (guild.members.has(user.id)) {
            return true;
        }
        else {
            try {
                await guild.members.fetch(user);
                return true;
            } catch (e) {
                Log.error(`Error occurred while fetching user ${Format.user(user)} for guild ${Format.guild(guild)} in GuildArgumentType parsing: ${e}`);
            }
        }

        return false;
    }
}

module.exports = GuildArgumentType;
