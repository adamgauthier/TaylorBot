import { Message } from 'discord.js';
import TaylorBotClient = require('../client/TaylorBotClient.js');

export abstract class MessageWatcher {
    enabled: boolean;
    constructor(enabled = true) {
        this.enabled = enabled;
    }

    abstract messageHandler(client: TaylorBotClient, message: Message): Promise<void>
}
