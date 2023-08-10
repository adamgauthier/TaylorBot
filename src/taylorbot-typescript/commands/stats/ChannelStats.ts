import { Command } from '../Command';
import { CommandMessageContext } from '../CommandMessageContext';

class ChannelStatsCommand extends Command {
    constructor() {
        super({
            name: 'channelstats',
            aliases: ['cstats'],
            group: 'Stats 📊',
            description: 'This command has been removed. Please use **/channel messages** instead.',
            examples: [''],

            args: [
                {
                    key: 'args',
                    label: 'args',
                    type: 'any-text',
                    prompt: 'What arguments would you like to use?'
                }
            ]
        });
    }

    async run({ message, client }: CommandMessageContext): Promise<void> {
        await client.sendEmbedError(message.channel, [
            `This command has been removed.`,
            `Please use **/channel messages** instead.`
        ].join('\n'));
    }
}

export = ChannelStatsCommand;
