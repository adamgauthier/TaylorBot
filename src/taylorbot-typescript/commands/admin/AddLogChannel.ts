import { Command } from '../Command';
import { CommandMessageContext } from '../CommandMessageContext';

class AddLogChannelCommand extends Command {
    constructor() {
        super({
            name: 'addlogchannel',
            aliases: ['alc'],
            group: 'admin',
            description: 'This command has been removed. Please use </monitor members set:887146682146488390> and </monitor deleted set:887146682146488390> instead.',
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
            `Please use </monitor members set:887146682146488390> and </monitor deleted set:887146682146488390> instead.`
        ].join('\n'));
    }
}

export = AddLogChannelCommand;
