import { EventHandler } from '../structures/EventHandler';
import Log = require('../tools/Logger.js');
import Format = require('../modules/DiscordFormatter.js');
import { TaylorBotClient } from '../client/TaylorBotClient';
import { Guild } from 'discord.js';

class GuildCreate extends EventHandler {
    constructor() {
        super('guildCreate');
    }

    handler(client: TaylorBotClient, guild: Guild): void {
        const { registry } = client.master;

        if (!registry.guilds.has(guild.id)) {
            Log.info(`Adding new guild ${Format.guild(guild)}.`);
            registry.guilds.cacheGuild({ guild_id: guild.id });
        }
    }
}

export = GuildCreate;
