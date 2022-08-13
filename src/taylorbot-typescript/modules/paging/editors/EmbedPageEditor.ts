import Discord = require('discord.js');
import { Message, EmbedBuilder } from 'discord.js';
import { TaylorBotClient } from '../../../client/TaylorBotClient';
import { PageEditor } from './PageEditor';

export abstract class EmbedPageEditor<T> extends PageEditor<T> {
    readonly embed: EmbedBuilder;

    constructor(embed: EmbedBuilder) {
        super();
        this.embed = embed;
    }

    sendMessage(client: TaylorBotClient, channel: Discord.PartialTextBasedChannelFields): Promise<Message> {
        return client.sendEmbed(channel, this.embed);
    }

    abstract update(pages: T[], currentPage: number): Promise<void>;

    edit(message: Message): Promise<Message> {
        return message.edit({ embeds: [ this.embed ] });
    }
}
