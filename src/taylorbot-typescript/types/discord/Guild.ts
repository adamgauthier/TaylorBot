import TextArgumentType = require('../base/Text');
import { ArgumentParsingError } from '../ArgumentParsingError';
import { Log } from '../../tools/Logger';
import { Format } from '../../modules/discord/DiscordFormatter';
import { CommandArgumentInfo, CommandMessageContext } from '../../commands/CommandMessageContext';
import { Guild, User } from 'discord.js';

class GuildArgumentType extends TextArgumentType {
    get id(): string {
        return 'guild';
    }

    async parse(val: string, { message, client }: CommandMessageContext, arg: CommandArgumentInfo): Promise<Guild> {
        const matches = val.trim().match(/^([0-9]+)$/);
        if (matches) {
            const guild = client.guilds.resolve(matches[1]);
            if (guild) {
                const isInGuild = await GuildArgumentType.isInGuild(message.author!, guild);
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
            const isInGuild = await GuildArgumentType.isInGuild(message.author!, guild!);
            if (isInGuild) {
                return guild!;
            }
            else {
                throw new ArgumentParsingError(`Could not find server '${val}' that you are a part of.`);
            }
        }

        const exactGuilds = guilds.filter(GuildArgumentType.guildFilterExact(search));

        for (const guild of exactGuilds.values()) {
            const isInGuild = await GuildArgumentType.isInGuild(message.author!, guild);
            if (isInGuild)
                return guild;
        }

        for (const guild of guilds.values()) {
            const isInGuild = await GuildArgumentType.isInGuild(message.author!, guild);
            if (isInGuild)
                return guild;
        }

        throw new ArgumentParsingError(`Could not find server '${val}' that you are a part of.`);
    }

    static guildFilterExact(search: string) {
        return (guild: Guild): boolean => guild.name.toLowerCase() === search;
    }

    static guildFilterInexact(search: string) {
        return (guild: Guild): boolean => guild.name.toLowerCase().includes(search);
    }

    static async isInGuild(user: User, guild: Guild): Promise<boolean> {
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

export = GuildArgumentType;
