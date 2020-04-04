import fsWithCallbacks = require('fs');
const fs = fsWithCallbacks.promises;
import path = require('path');
import { TaylorBotClient } from '../client/TaylorBotClient';
import { EventHandler } from '../structures/EventHandler';

const eventsPath = path.join(__dirname, '..', 'events');

export class EventLoader {
    static async loadAll(client: TaylorBotClient): Promise<void> {
        const files = await fs.readdir(eventsPath);

        return files
            .map(file => path.join(eventsPath, file))
            .map(path.parse)
            .filter(file => file.ext === '.js')
            .map(path.format)
            .map(file => require(file))
            .map((handler: new () => EventHandler) => new handler())
            .filter(event => event.enabled)
            .forEach(event =>
                client.on(event.eventName, (...args: any[]) => event.handler(client, ...args))
            );
    }
}
