import Discord = require('discord.js');

import { EventLoader } from '../modules/EventLoader';
import DISCORD_CONFIG = require('../config/discord.json');
import Log = require('../tools/Logger.js');
import { IntervalRunner } from '../intervals/IntervalRunner';
import EmbedUtil = require('../modules/EmbedUtil.js');
import { TaylorBotMasterClient } from './TaylorBotMasterClient';

export class TaylorBotClient extends Discord.Client {
    master: TaylorBotMasterClient;
    intervalRunner: IntervalRunner;
    constructor(master: TaylorBotMasterClient) {
        super({
            'disabledEvents': ['TYPING_START']
        });

        this.master = master;

        this.intervalRunner = new IntervalRunner(this);
    }

    async load(): Promise<void> {
        Log.info('Loading events...');
        await EventLoader.loadAll(this);
        Log.info('Events loaded!');

        Log.info('Loading intervals...');
        await this.intervalRunner.loadAll();
        Log.info('Intervals loaded!');
    }

    async start(): Promise<void> {
        await this.login(DISCORD_CONFIG.loginToken);
    }

    sendMessage(recipient: Discord.PartialTextBasedChannelFields, text: string, options: Discord.MessageOptions | Discord.MessageAdditions): Promise<Discord.Message> {
        return recipient.send(
            text.replace(/(@)(everyone|here)/g, (m, p1, p2) => `${p1}\u200B${p2}`),
            options
        );
    }

    sendEmbed(recipient: Discord.PartialTextBasedChannelFields, embed: Discord.MessageEmbed): Promise<Discord.Message> {
        const options = { embed };

        return this.sendMessage(recipient, '', options);
    }

    sendEmbedError(recipient: Discord.PartialTextBasedChannelFields, errorMessage: string): Promise<Discord.Message> {
        return this.sendEmbed(recipient, EmbedUtil.error(errorMessage));
    }

    sendEmbedSuccess(recipient: Discord.PartialTextBasedChannelFields, successMessage: string): Promise<Discord.Message> {
        return this.sendEmbed(recipient, EmbedUtil.success(successMessage));
    }
}
