'use strict';

const EventEmitter = require('events');

const DiscordEmbedFormatter = require('../DiscordEmbedFormatter.js');
const StringUtil = require('../StringUtil.js');
const Option = require('./Option.js');

class Poll extends EventEmitter {
    constructor(client, channel, owner, options) {
        super();
        this.client = client;
        this.channel = channel;
        this.owner = owner;
        this.options = options.reduce(
            (map, name, index) => map.set(index + 1, new Option(name)),
            new Map()
        );
        this.votes = new Map();
        this.duration = 10 * 60 * 1000;
    }

    resetCloseTimeout() {
        clearTimeout(this.closeTimeout);
        this.endsAt = Date.now() + this.duration;
        this.closeTimeout = setTimeout(() => this.emit('close'), this.duration);
    }

    async send() {
        await this.client.sendEmbed(this.channel, DiscordEmbedFormatter
            .baseUserEmbed(this.owner)
            .setTitle(`Poll '${this.channel.name}' started!`)
            .setDescription(Array.from(
                this.options.entries(),
                ([key, option]) => `**${key}**: ${option.name}`
            ).join('\n'))
            .setFooter('Type a number to vote!')
        );
        this.resetCloseTimeout();
    }

    close() {
        clearTimeout(this.closeTimeout);
        return this.client.sendEmbed(this.channel, DiscordEmbedFormatter
            .baseUserEmbed(this.owner)
            .setTitle(`Poll '${this.channel.name}' closed! Results:`)
            .setDescription(Array.from(
                this.options.values(),
                option => `**${option.name}**: ${StringUtil.plural(option.voteCount, 'vote')}`
            ).join('\n'))
        );
    }

    show() {
        return this.client.sendEmbed(this.channel, DiscordEmbedFormatter
            .baseUserEmbed(this.owner)
            .setTitle(`Poll '${this.channel.name}' current results:`)
            .setDescription(Array.from(
                this.options.entries(),
                ([key, option]) => `**${key}**: ${option.name} - ${StringUtil.plural(option.voteCount, 'vote')}`
            ).join('\n'))
            .setFooter('Type a number to vote!')
        );
    }

    canClose(user) {
        return this.owner.id === user.id;
    }

    vote(user, number) {
        const option = this.options.get(number);
        if (option) {
            const oldVote = this.votes.get(user.id);
            if (oldVote) {
                this.options.get(oldVote).decrementVoteCount();
            }

            this.votes.set(user.id, number);
            this.options.get(number).incrementVoteCount();
            this.resetCloseTimeout();
        }
    }
}

module.exports = Poll;