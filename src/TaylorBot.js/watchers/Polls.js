'use strict';

const MessageWatcher = require('../structures/MessageWatcher.js');
const Poll = require('../modules/poll/Poll.js');

class PollsWatcher extends MessageWatcher {
    constructor() {
        super();
        this.polls = new Map();
    }

    hasPoll(channel) {
        return this.polls.has(channel.id);
    }

    getPoll(channel) {
        return this.polls.get(channel.id);
    }

    startPoll(client, channel, owner, options) {
        const poll = new Poll(client, channel, owner, options);

        poll.once('close', () => this.stopPoll(channel));

        this.polls.set(channel.id, poll);

        return poll.send();
    }

    showPoll(channel) {
        const poll = this.polls.get(channel.id);

        return poll.show();
    }

    stopPoll(channel) {
        const poll = this.polls.get(channel.id);

        if (this.polls.delete(channel.id)) {
            return poll.close();
        }
    }

    async messageHandler(client, message) {
        const { author, channel, content } = message;
        const poll = this.polls.get(channel.id);
        if (poll && !author.bot) {
            const vote = Number.parseInt(content);

            if (!Number.isNaN(vote) && vote >= 0) {
                poll.vote(author, vote);
            }
        }
    }
}

module.exports = PollsWatcher;
