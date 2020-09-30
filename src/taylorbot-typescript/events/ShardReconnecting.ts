import { EventHandler } from '../structures/EventHandler';
import { Log } from '../tools/Logger';
import { TaylorBotClient } from '../client/TaylorBotClient';

class ShardReconnecting extends EventHandler {
    constructor() {
        super('shardReconnecting');
    }

    handler(taylorbot: TaylorBotClient, id: number): void {
        Log.info(`Shard ${id} is attempting to reconnect.`);
    }
}

export = ShardReconnecting;
