'use strict';

const PREVIOUS_EMOJI = '◀';
const NEXT_EMOJI = '▶';

class PageMessage {
    constructor(client, owner, pages, editor) {
        this.client = client;
        this.owner = owner;
        this.pages = pages;
        this.editor = editor;
        this.currentPage = 0;
        this.duration = 30 * 1000;
        this.closeTimeout = null;
        this.collector = null;
        this.message = null;
    }

    setCloseTimeout() {
        this.closeTimeout = setTimeout(() => this.collector.stop(), this.duration);
    }

    async send(channel) {
        await this.editor.update(this.pages, this.currentPage);
        this.message = await this.editor.sendMessage(this.client, channel);

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
            await this.editor.update(this.pages, this.currentPage);
            return this.editor.edit(this.message);
        }
    }

    async next() {
        let newPage = this.currentPage + 1;

        if (newPage > this.pages.length - 1) {
            newPage = 0;
        }

        if (this.currentPage !== newPage) {
            this.currentPage = newPage;
            await this.editor.update(this.pages, this.currentPage);
            return this.editor.edit(this.message);
        }
    }
}

module.exports = PageMessage;