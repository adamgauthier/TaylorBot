import { TextChannel } from 'discord.js';
import PollsWatcher = require('../../watchers/Polls');
import { Command } from '../Command';
import { CommandError } from '../CommandError';
import { CommandMessageContext } from '../CommandMessageContext';

class PollCommand extends Command {
    constructor() {
        super({
            name: 'poll',
            group: 'Fun 🎭',
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

    async run({ message, client, author, messageContext }: CommandMessageContext, { options }: { options: string[]  }): Promise<void> {
        const channel = message.channel as TextChannel;
        const { watchers } = client.master.registry;

        const pollsWatcher = watchers.getWatcher('Polls') as PollsWatcher;

        if (pollsWatcher.hasPoll(channel)) {
            throw new CommandError(`There is already a poll in ${channel}. Use the \`${messageContext.prefix}closepoll\` command to close it first.`);
        }

        await pollsWatcher.startPoll(
            client, channel, author, options
        );
    }
}

export = PollCommand;
