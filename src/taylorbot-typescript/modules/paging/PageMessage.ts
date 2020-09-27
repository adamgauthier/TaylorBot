import { Message, MessageReaction, ReactionCollector, User } from 'discord.js';
import { TaylorBotClient } from '../../client/TaylorBotClient';
import { PageEditor } from './editors/PageEditor';
import Discord = require('discord.js');

const PREVIOUS_EMOJI = '◀';
const NEXT_EMOJI = '▶';
const CANCEL_EMOJI = '❌';

export class PageMessage<T> {
    client: TaylorBotClient;
    owner: User;
    pages: T[];
    editor: PageEditor<T>;
    cancellable: boolean;
    currentPage: number;
    duration: number;
    closeTimeout: NodeJS.Timeout | null;
    collector: ReactionCollector | null;
    message: Message | null;
    buttons: string[];

    constructor(client: TaylorBotClient, owner: User, pages: T[], editor: PageEditor<T>, { cancellable = false } = {}) {
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

    setCloseTimeout(): void {
        this.closeTimeout = setTimeout(() => this.collector!.stop(), this.duration);
    }

    async send(channel: Discord.PartialTextBasedChannelFields): Promise<void> {
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
                this.buttons.map(button => this.message!.react(button))
            );
        }
    }

    filter(reaction: MessageReaction, user: User): boolean {
        return user.id === this.owner.id && this.buttons.includes(reaction.emoji.name);
    }

    onReact(reaction: MessageReaction): void {
        clearTimeout(this.closeTimeout!);
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

    async previous(): Promise<void> {
        let newPage = this.currentPage - 1;

        if (newPage < 0) {
            newPage = this.pages.length - 1;
        }

        if (this.currentPage !== newPage) {
            this.currentPage = newPage;
            await this.editor.update(this.pages, this.currentPage);
            await this.editor.edit(this.message!);
        }
    }

    async next(): Promise<void> {
        let newPage = this.currentPage + 1;

        if (newPage > this.pages.length - 1) {
            newPage = 0;
        }

        if (this.currentPage !== newPage) {
            this.currentPage = newPage;
            await this.editor.update(this.pages, this.currentPage);
            await this.editor.edit(this.message!);
        }
    }

    async cancel(): Promise<void> {
        await this.message!.delete();
    }
}
