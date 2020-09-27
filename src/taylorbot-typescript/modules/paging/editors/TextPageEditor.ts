import Discord = require('discord.js');
import { TaylorBotClient } from '../../../client/TaylorBotClient';
import { PageEditor } from './PageEditor';

export class TextPageEditor extends PageEditor<string> {
    #text: string | null;

    constructor() {
        super();
        this.#text = null;
    }

    sendMessage(client: TaylorBotClient, channel: Discord.PartialTextBasedChannelFields): Promise<Discord.Message> {
        return client.sendMessage(channel, this.#text!, undefined);
    }

    update(pages: string[], currentPage: number): Promise<void> {
        this.#text = pages[currentPage];
        return Promise.resolve();
    }

    edit(message: Discord.Message): Promise<Discord.Message> {
        return message.edit(this.#text);
    }
}
