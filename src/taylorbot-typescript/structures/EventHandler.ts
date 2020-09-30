import { ClientEvents } from 'discord.js';
import { TaylorBotClient } from '../client/TaylorBotClient';

export abstract class EventHandler {
    constructor(public eventName: keyof ClientEvents, public enabled = true) {
    }

    abstract handler(client: TaylorBotClient, ...args: any[]): Promise<void> | void;
}
