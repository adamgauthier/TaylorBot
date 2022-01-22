import Discord = require('discord.js');
import { EmbedUtil } from '../discord/EmbedUtil';
import { PageMessage } from './PageMessage';

export class DeprecatedCommandPageMessage extends PageMessage<string> {
    #newCommandName: string;

    constructor(newCommandName: string) {
        super(null!, null!, null!, null!, {});
        this.#newCommandName = newCommandName;
    }

    async send(channel: Discord.PartialTextBasedChannelFields): Promise<void> {
        await channel.send({ embeds: [EmbedUtil.error(`This command has been removed. Please use **${this.#newCommandName}** instead.`)] });
    }
}
