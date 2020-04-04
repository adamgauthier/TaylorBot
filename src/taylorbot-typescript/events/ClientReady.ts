import Log = require('../tools/Logger.js');
import Format = require('../modules/DiscordFormatter.js');
import { EventHandler } from '../structures/EventHandler';
import { TaylorBotClient } from '../client/TaylorBotClient';

class ClientReady extends EventHandler {
    constructor() {
        super('ready');
    }

    async handler(client: TaylorBotClient): Promise<void> {
        Log.info('Client is ready!');

        client.intervalRunner.startAll();
        Log.info('Intervals started!');

        Log.info('Caching new guilds...');
        this.syncRegistry(client);
        Log.info('New guilds cached!');
    }

    syncRegistry(client: TaylorBotClient): void {
        const { registry } = client.master;

        for (const guild of client.guilds.values()) {
            if (!registry.guilds.has(guild.id)) {
                Log.info(`Adding new guild ${Format.guild(guild)}.`);
                registry.guilds.cacheGuild({ guild_id: guild.id });
            }
        }
    }
}

export = ClientReady;
