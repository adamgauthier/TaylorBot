import { Message } from 'discord.js';
import { TaylorBotClient } from '../client/TaylorBotClient';

export class MessageContext {
    prefix = '';
    wasOnGoingCommandAdded = false;
    constructor(public readonly message: Message, public readonly client: TaylorBotClient) {
    }

    get isGuild(): boolean {
        return this.message.channel.type === 'text';
    }
}
