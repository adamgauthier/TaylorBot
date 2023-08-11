import { Command } from '../Command';
import { CommandMessageContext } from '../CommandMessageContext';

class RemoveLogChannelCommand extends Command {
    constructor() {
        super({
            name: 'removelogchannel',
            aliases: ['rlc'],
            group: 'admin',
            description: 'This command has been removed. Please use </monitor members stop:887146682146488390> and </monitor deleted stop:887146682146488390> instead.',
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
            `Please use </monitor members stop:887146682146488390> and </monitor deleted stop:887146682146488390> instead.`
        ].join('\n'));
    }
}

export = RemoveLogChannelCommand;
