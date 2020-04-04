'use strict';

const PREVIOUS_EMOJI = '◀';
const NEXT_EMOJI = '▶';
const CANCEL_EMOJI = '❌';

class PageMessage {
    constructor(client, owner, pages, editor, { cancellable = false } = {}) {
        this.client = client;
        this.owner = owner;
        this.pages = pages;
        this.editor = editor;
        this.cancellable = cancellable;
        this.currentPage = 0;
        this.duration = 30 * 1000;
        this.closeTimeout = null;
        this.collector = null;
        this.message = null;
        this.buttons = [];
    }

    setCloseTimeout() {
        this.closeTimeout = setTimeout(() => this.collector.stop(), this.duration);
    }

    async send(channel) {
        await this.editor.update(this.pages, this.currentPage);
        this.message = await this.editor.sendMessage(this.client, channel);

        if (this.pages.length > 1) {
            this.buttons.push(PREVIOUS_EMOJI, NEXT_EMOJI);
        }

        if (this.cancellable) {
            this.buttons.push(CANCEL_EMOJI);
        }

        if (this.buttons.length > 0) {
            this.collector = this.message.createReactionCollector((reaction, user) => this.filter(reaction, user), { dispose: true });

            this.collector
                .on('collect', reaction => this.onReact(reaction))
                .on('remove', reaction => this.onReact(reaction));

            this.setCloseTimeout();

            await Promise.all(
                this.buttons.map(button => this.message.react(button))
            );
        }
    }

    filter(reaction, user) {
        return user.id === this.owner.id && this.buttons.includes(reaction.emoji.name);
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
            case CANCEL_EMOJI:
                this.cancel();
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

    cancel() {
        return this.message.delete();
    }
}

module.exports = PageMessage;