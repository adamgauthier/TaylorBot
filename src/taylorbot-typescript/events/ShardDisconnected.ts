import { EventHandler } from '../structures/EventHandler';
import { Log } from '../tools/Logger';
import { TaylorBotClient } from '../client/TaylorBotClient';

class ShardDisconnected extends EventHandler {
    constructor() {
        super('shardDisconnected');
    }

    handler(taylorbot: TaylorBotClient, closeEvent: CloseEvent, id: number): void {
        Log.info(`Shard ${id} disconnected! Reason: ${closeEvent.code} - ${closeEvent.reason}`);

        taylorbot.intervalRunner.stopAll();
        Log.info('Intervals stopped!');
    }
}

export = ShardDisconnected;
