'use strict';

const Command = require('../Command.js');
const CommandError = require('../CommandError.js');

class ShowPollCommand extends Command {
    constructor() {
        super({
            name: 'showpoll',
            aliases: ['spoll', 'pollshow'],
            group: 'fun',
            description: 'Shows the current results of a poll in a channel!',
            examples: [''],
            guildOnly: true,

            args: []
        });
    }

    async run({ message: { channel }, client, messageContext: { guildSettings: { prefix } } }) {
        const { watchers } = client.master.registry;

        const pollsWatcher = watchers.getWatcher('Polls');

        if (!pollsWatcher.hasPoll(channel)) {
            throw new CommandError(`There is no poll in ${channel}. Use the \`${prefix}poll\` command to start one!`);
        }

        return pollsWatcher.showPoll(channel);
    }
}

module.exports = ShowPollCommand;
