import Discord = require('discord.js');
import { Message } from 'discord.js';
import { TaylorBotClient } from '../../../client/TaylorBotClient';

export abstract class PageEditor<T> {
    abstract sendMessage(client: TaylorBotClient, channel: Discord.PartialTextBasedChannelFields): Promise<Message>;

    abstract update(pages: T[], currentPage: number): Promise<void>;

    abstract edit(message: Message): Promise<Message>;
}
