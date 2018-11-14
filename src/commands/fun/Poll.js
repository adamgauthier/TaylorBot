'use strict';

const Command = require('../Command.js');
const CommandError = require('../CommandError.js');

class PollCommand extends Command {
    constructor() {
        super({
            name: 'poll',
            group: 'fun',
            description: 'Create a poll in a channel! Use the `closepoll` command to close a poll!',
            examples: ['Cake, Pie'],
            guildOnly: true,

            args: [
                {
                    key: 'options',
                    label: 'option1,option2,...',
                    type: 'poll-options',
                    prompt: 'What are the options (comma separated) for your poll?'
                }
            ]
        });
    }

    async run({ message: { author, channel }, client, messageContext: { guildSettings: { prefix } } }, { options }) {
        const { watchers } = client.master.registry;

        const pollsWatcher = watchers.getWatcher('Polls');

        if (pollsWatcher.hasPoll(channel)) {
            throw new CommandError(`There is already a poll in ${channel}. Use the \`${prefix}closepoll\` command to close it first.`);
        }

        return pollsWatcher.startPoll(
            client, channel, author, options
        );
    }
}

module.exports = PollCommand;