import { EventHandler } from '../structures/EventHandler';
import { TaylorBotClient } from '../client/TaylorBotClient';
import { Message as DiscordMessage } from 'discord.js';

class Message extends EventHandler {
    constructor() {
        super('messageCreate');
    }

    handler(client: TaylorBotClient, message: DiscordMessage): void {
        if (!message.partial)
            client.master.registry.watchers.feedAll(client, message);
    }
}

export = Message;
