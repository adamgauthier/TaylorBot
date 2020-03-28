import { EventHandler } from '../structures/EventHandler';
import Log = require('../tools/Logger.js');
import Format = require('../modules/DiscordFormatter.js');
import { Guild } from 'discord.js';
import { TaylorBotClient } from '../client/TaylorBotClient';

class GuildDelete extends EventHandler {
    constructor() {
        super('guildDelete');
    }

    handler(client: TaylorBotClient, guild: Guild): void {
        Log.info(`Guild ${Format.guild(guild)} was deleted, or client left it.`);
    }
}

export = GuildDelete;
