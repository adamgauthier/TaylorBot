import { EventHandler } from '../structures/EventHandler';
import { Log } from '../tools/Logger';
import { TaylorBotClient } from '../client/TaylorBotClient';

class ShardResume extends EventHandler {
    constructor() {
        super('shardResume');
    }

    handler(taylorbot: TaylorBotClient, id: number, replayedEvents: number): void {
        Log.info(`Shard ${id} resumed successfully: ${replayedEvents} replayed event(s).`);
    }
}

export = ShardResume;
