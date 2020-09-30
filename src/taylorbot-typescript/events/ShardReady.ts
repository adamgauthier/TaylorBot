import { EventHandler } from '../structures/EventHandler';
import { Log } from '../tools/Logger';
import { TaylorBotClient } from '../client/TaylorBotClient';
import { Snowflake } from 'discord.js';

class ShardReady extends EventHandler {
    constructor() {
        super('shardReady');
    }

    handler(taylorbot: TaylorBotClient, id: number, unavailableGuilds: Set<Snowflake> | undefined): void {
        Log.info(`Shard ${id} is ready with ${unavailableGuilds !== undefined ? unavailableGuilds.size : 0} unavailable guild(s)!`);
    }
}

export = ShardReady;
