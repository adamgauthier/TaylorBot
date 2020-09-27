import events = require('events');

import { DiscordEmbedFormatter } from '../discord/DiscordEmbedFormatter';
import { StringUtil } from '../util/StringUtil';
import { Option } from './Option';
import DISCORD_CONFIG = require('../../config/config.json');
import { TaylorBotClient } from '../../client/TaylorBotClient';
import { User, TextChannel } from 'discord.js';

export class Poll extends events.EventEmitter {
    readonly #options: Map<number, Option>;
    readonly #duration = 10 * 60 * 1000;
    readonly #votes = new Map<string, number>();
    #closeTimeout: NodeJS.Timeout | null = null;
    endsAt: number | null = null;

    constructor(private client: TaylorBotClient, private channel: TextChannel, public owner: User, options: string[]) {
        super();
        this.#options = options.reduce(
            (map, name, index) => map.set(index + 1, new Option(name)),
            new Map<number, Option>()
        );
    }

    private getRequiredOption(number: number): Option {
        const option = this.#options.get(number);
        if (option === undefined) throw new Error(`No option for vote ${number}.`);
        return option;
    }

    setCloseTimeout(): void {
        this.endsAt = Date.now() + this.#duration;
        this.#closeTimeout = setTimeout(() => this.emit('close'), this.#duration);
    }

    async send(): Promise<void> {
        await this.client.sendEmbed(this.channel, DiscordEmbedFormatter
            .baseUserEmbed(this.owner)
            .setTitle(`Poll '${this.channel.name}' started!`)
            .setDescription(Array.from(
                this.#options.entries(),
                ([key, option]) => `**${key}**: ${option.name}`
            ).join('\n'))
            .setFooter('Type a number to vote!')
        );
        this.setCloseTimeout();
    }

    async close(): Promise<void> {
        if (this.#closeTimeout == null) throw new Error(`Can't close a poll that hasn't been started.`);

        clearTimeout(this.#closeTimeout);
        await this.client.sendEmbed(this.channel, DiscordEmbedFormatter
            .baseUserEmbed(this.owner)
            .setTitle(`Poll '${this.channel.name}' closed! Results:`)
            .setDescription(Array.from(
                this.#options.values(),
                option => `**${option.name}**: ${StringUtil.plural(option.voteCount, 'vote')}`
            ).join('\n'))
        );
    }

    async show(): Promise<void> {
        await this.client.sendEmbed(this.channel, DiscordEmbedFormatter
            .baseUserEmbed(this.owner)
            .setTitle(`Poll '${this.channel.name}' current results:`)
            .setDescription(Array.from(
                this.#options.entries(),
                ([key, option]) => `**${key}**: ${option.name} - ${StringUtil.plural(option.voteCount, 'vote')}`
            ).join('\n'))
            .setFooter('Type a number to vote!')
        );
    }

    canClose(user: User): boolean {
        return this.owner.id === user.id || DISCORD_CONFIG.MASTER_ID === user.id;
    }

    vote(user: User, number: number): void {
        if (number === 0) {
            const oldVote = this.#votes.get(user.id);
            if (oldVote) {
                this.getRequiredOption(oldVote).decrementVoteCount();
                this.#votes.delete(user.id);
            }
        }
        else {
            const option = this.#options.get(number);
            if (option) {
                const oldVote = this.#votes.get(user.id);
                if (oldVote) {
                    this.getRequiredOption(oldVote).decrementVoteCount();
                }

                this.#votes.set(user.id, number);
                option.incrementVoteCount();

                if (this.#closeTimeout == null) throw new Error(`Can't vote in a poll that hasn't been started.`);
                clearTimeout(this.#closeTimeout);
                this.setCloseTimeout();
            }
        }
    }
}
