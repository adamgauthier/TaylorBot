import Discord = require('discord.js');

import { EventLoader } from '../modules/discord/EventLoader';
import { Log } from '../tools/Logger';
import { IntervalRunner } from '../intervals/IntervalRunner';
import { EmbedUtil } from '../modules/discord/EmbedUtil';
import { TaylorBotMasterClient } from './TaylorBotMasterClient';
import { EnvUtil } from '../modules/util/EnvUtil';
import { GatewayIntentBits, Partials, Options } from 'discord.js';

const discordToken = EnvUtil.getRequiredEnvVariable('TaylorBot_Discord__Token');

export class TaylorBotClient extends Discord.Client {
    readonly master: TaylorBotMasterClient;
    readonly intervalRunner: IntervalRunner;

    constructor(master: TaylorBotMasterClient) {
        const cacheSettings = Options.DefaultMakeCacheSettings;
        cacheSettings.MessageManager = 0;
        super({
            shards: 'auto',
            allowedMentions: { parse: [], repliedUser: true },
            partials: [Partials.Reaction, Partials.Message, Partials.Channel],
            intents: [
                GatewayIntentBits.MessageContent,
                GatewayIntentBits.Guilds,
                GatewayIntentBits.GuildMembers,
                GatewayIntentBits.GuildMessages,
                GatewayIntentBits.GuildMessageReactions,
                GatewayIntentBits.DirectMessages,
                GatewayIntentBits.DirectMessageReactions,
            ],
            makeCache: Options.cacheWithLimits(cacheSettings),
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
        await this.login(discordToken);
    }

    sendEmbed(recipient: Discord.PartialTextBasedChannelFields, embed: Discord.EmbedBuilder): Promise<Discord.Message> {
        return recipient.send({ embeds: [embed] });
    }

    sendEmbedError(recipient: Discord.PartialTextBasedChannelFields, errorMessage: string): Promise<Discord.Message> {
        return this.sendEmbed(recipient, EmbedUtil.error(errorMessage));
    }

    sendEmbedSuccess(recipient: Discord.PartialTextBasedChannelFields, successMessage: string): Promise<Discord.Message> {
        return this.sendEmbed(recipient, EmbedUtil.success(successMessage));
    }
}
