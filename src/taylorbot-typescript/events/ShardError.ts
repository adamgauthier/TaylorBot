import { EventHandler } from '../structures/EventHandler';
import { Log } from '../tools/Logger';
import { TaylorBotClient } from '../client/TaylorBotClient';

class ShardError extends EventHandler {
    constructor() {
        super('shardError');
    }

    handler(taylorbot: TaylorBotClient, error: Error, shardId: number): void {
        Log.error(`Shard ${shardId} WebSocket error encountered: ${error}`);
    }
}

export = ShardError;
