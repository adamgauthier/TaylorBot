import { TextChannel } from 'discord.js';
import PollsWatcher = require('../../watchers/Polls');
import { Command } from '../Command';
import { CommandError } from '../CommandError';
import { CommandMessageContext } from '../CommandMessageContext';

class ShowPollCommand extends Command {
    constructor() {
        super({
            name: 'showpoll',
            aliases: ['spoll', 'pollshow'],
            group: 'Fun 🎭',
            description: 'Shows the current results of a poll in a channel!',
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

        await pollsWatcher.showPoll(channel);
    }
}

export = ShowPollCommand;
