import { MessageWatcher } from '../structures/MessageWatcher';
import { Poll } from '../modules/poll/Poll';
import { Message, User, TextChannel } from 'discord.js';
import { TaylorBotClient } from '../client/TaylorBotClient';

class PollsWatcher extends MessageWatcher {
    #polls = new Map<string, Poll>();

    hasPoll(channel: TextChannel): boolean {
        return this.#polls.has(channel.id);
    }

    private getRequiredPoll(channel: TextChannel): Poll {
        const maybePoll = this.getPoll(channel);
        if (maybePoll === undefined) throw new Error(`No poll for channel ${channel.id}.`);
        return maybePoll;
    }

    getPoll(channel: TextChannel): Poll | undefined {
        return this.#polls.get(channel.id);
    }

    startPoll(client: TaylorBotClient, channel: TextChannel, owner: User, options: string[]): Promise<void> {
        const poll = new Poll(client, channel, owner, options);

        poll.once('close', () => this.stopPoll(channel));

        this.#polls.set(channel.id, poll);

        return poll.send();
    }

    showPoll(channel: TextChannel): Promise<void> {
        const poll = this.getRequiredPoll(channel);

        return poll.show();
    }

    async stopPoll(channel: TextChannel): Promise<void> {
        const poll = this.getRequiredPoll(channel);

        if (this.#polls.delete(channel.id)) {
            await poll.close();
        }
    }

    messageHandler(client: TaylorBotClient, message: Message): Promise<void> {
        const { author, channel, content } = message;
        if (author == null) throw new Error(`Message ${message.id} has no author.`);

        const poll = this.#polls.get(channel.id);
        if (poll && !author.bot) {
            const vote = Number.parseInt(content);

            if (!Number.isNaN(vote) && vote >= 0) {
                poll.vote(author, vote);
            }
        }

        return Promise.resolve();
    }
}

export = PollsWatcher;
