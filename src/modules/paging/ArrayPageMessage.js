'use strict';

const PREVIOUS_EMOJI = '◀';
const NEXT_EMOJI = '▶';

class ArrayPageMessage {
    constructor(client, owner, pages) {
        if (new.target === ArrayPageMessage) {
            throw new Error(`Can't instantiate abstract ${this.constructor.name} class.`);
        }

        this.client = client;
        this.owner = owner;
        this.pages = pages;
        this.currentPage = 0;
        this.duration = 30 * 1000;
        this.closeTimeout = null;
        this.collector = null;
    }

    setCloseTimeout() {
        this.closeTimeout = setTimeout(() => this.collector.stop(), this.duration);
    }

    async send(channel) {
        await this.update();
        this.message = await this.sendMessage(channel);

        if (this.pages.length > 1) {
            this.collector = this.message.createReactionCollector(this.filter(), { dispose: true });

            this.collector
                .on('collect', reaction => this.onReact(reaction))
                .on('remove', reaction => this.onReact(reaction));

            this.setCloseTimeout();

            await this.message.react(PREVIOUS_EMOJI);
            return this.message.react(NEXT_EMOJI);
        }
    }

    filter() {
        return (reaction, user) =>
            user.id === this.owner.id &&
            [PREVIOUS_EMOJI, NEXT_EMOJI].includes(reaction.emoji.name);
    }

    onReact(reaction) {
        clearTimeout(this.closeTimeout);
        this.setCloseTimeout();

        switch (reaction.emoji.name) {
            case PREVIOUS_EMOJI:
                this.previous();
                break;
            case NEXT_EMOJI:
                this.next();
                break;
        }
    }

    async previous() {
        let newPage = this.currentPage - 1;

        if (newPage < 0) {
            newPage = this.pages.length - 1;
        }

        if (this.currentPage !== newPage) {
            this.currentPage = newPage;
            await this.update();
            return this.edit();
        }
    }

    async next() {
        let newPage = this.currentPage + 1;

        if (newPage > this.pages.length - 1) {
            newPage = 0;
        }

        if (this.currentPage !== newPage) {
            this.currentPage = newPage;
            await this.update();
            return this.edit();
        }
    }

    sendMessage(channel) { // eslint-disable-line no-unused-vars
        throw new Error(`${this.constructor.name} doesn't have an sendMessage() method.`);
    }

    update() {
        throw new Error(`${this.constructor.name} doesn't have an update() method.`);
    }

    edit() {
        throw new Error(`${this.constructor.name} doesn't have an edit() method.`);
    }
}

module.exports = ArrayPageMessage;