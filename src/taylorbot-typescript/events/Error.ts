import { EventHandler } from '../structures/EventHandler.js';
import { Log } from '../tools/Logger';
import { TaylorBotClient } from '../client/TaylorBotClient.js';

class ErrorHandler extends EventHandler {
    constructor() {
        super('error');
    }

    handler(client: TaylorBotClient, error: Error): void {
        Log.error(`Client WebSocket error encountered: ${error}`);
    }
}

export = ErrorHandler;
