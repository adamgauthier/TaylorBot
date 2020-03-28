import { Message } from 'discord.js';
import { TaylorBotClient } from '../client/TaylorBotClient';

export abstract class MessageWatcher {
    enabled: boolean;
    constructor(enabled = true) {
        this.enabled = enabled;
    }

    abstract messageHandler(client: TaylorBotClient, message: Message): Promise<void>
}
