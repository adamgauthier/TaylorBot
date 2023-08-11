import { Command } from '../Command';
import { CommandMessageContext } from '../CommandMessageContext';

class ServerNamesCommand extends Command {
    constructor() {
        super({
            name: 'servernames',
            aliases: ['snames', 'guildnames', 'gnames'],
            group: 'Stats 📊',
            description: 'This command has been removed. Please use </server names:1137547317549998130> instead.',
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
            `Please use </server names:1137547317549998130> instead.`
        ].join('\n'));
    }
}

export = ServerNamesCommand;
