import { Constants } from 'discord.js';
import { TaylorBotClient } from '../client/TaylorBotClient';

type ValueOf<T> = T[keyof T];

export abstract class EventHandler {
    constructor(public eventName: ValueOf<Constants['Events']>, public enabled = true) {
    }

    abstract handler(client: TaylorBotClient, ...args: any[]): Promise<void> | void;
}
