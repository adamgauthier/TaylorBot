import moment = require('moment');

import { Command } from '../Command';
import { CommandError } from '../CommandError';
import { CommandMessageContext } from '../CommandMessageContext';
import PollsWatcher = require('../../watchers/Polls');
import { TextChannel } from 'discord.js';

class ClosePollCommand extends Command {
    constructor() {
        super({
            name: 'closepoll',
            aliases: ['cpoll', 'pollclose'],
            group: 'Fun 🎭',
            description: 'Closes a poll in a channel!',
            examples: [''],
            guildOnly: true,

            args: []
        });
    }

    async run({ message, client, messageContext }: CommandMessageContext): Promise<void> {
        const channel = message.channel as TextChannel;
        const { watchers } = client.master.registry;

        const pollsWatcher = watchers.getWatcher('Polls') as PollsWatcher;

        if (!pollsWatcher.hasPoll(channel)) {
            throw new CommandError(`There is no poll in ${channel}. Use the \`${messageContext.prefix}poll\` command to start one!`);
        }

        const poll = pollsWatcher.getPoll(channel)!;

        if (!poll.canClose(message.member!)) {
            throw new CommandError([
                `This poll can only be closed by ${poll.owner} or members with the permission "Manage Messages".`,
                `Otherwise, it will be automatically closed ${moment.utc(poll.endsAt!).fromNow()}.`
            ].join('\n'));
        }

        await pollsWatcher.stopPoll(channel);
    }
}

export = ClosePollCommand;
