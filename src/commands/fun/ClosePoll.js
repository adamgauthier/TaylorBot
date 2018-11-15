'use strict';

const moment = require('moment');

const Command = require('../Command.js');
const CommandError = require('../CommandError.js');

class ClosePollCommand extends Command {
    constructor() {
        super({
            name: 'closepoll',
            aliases: ['cpoll'],
            group: 'fun',
            description: 'Closes a poll in a channel!',
            examples: [''],
            guildOnly: true,

            args: []
        });
    }

    async run({ message: { author, channel }, client, messageContext: { guildSettings: { prefix } } }) {
        const { watchers } = client.master.registry;

        const pollsWatcher = watchers.getWatcher('Polls');

        if (!pollsWatcher.hasPoll(channel)) {
            throw new CommandError(`There is no poll in ${channel}. Use the \`${prefix}poll\` command to start one!`);
        }

        const poll = pollsWatcher.getPoll(channel);

        if (!poll.canClose(author)) {
            throw new CommandError(
                `This poll can only be closed by ${poll.owner}. Otherwise, it will be closed ${moment.utc(poll.endsAt()).fromNow()}.`
            );
        }

        return pollsWatcher.stopPoll(channel);
    }
}

module.exports = ClosePollCommand;