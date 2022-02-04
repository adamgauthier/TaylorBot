import { Command } from '../Command';
import { CommandMessageContext } from '../CommandMessageContext';

class AddLogChannelCommand extends Command {
    constructor() {
        super({
            name: 'addlogchannel',
            aliases: ['alc'],
            group: 'admin',
            description: 'This command has been removed. Please use the `log member` or `log deleted` commands instead.',
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

    async run({ message, client, messageContext }: CommandMessageContext, { args }: { args: string }): Promise<void> {
        await client.sendEmbedError(message.channel, [
            `This command has been removed.`,
            `Please use \`${messageContext.prefix}log member\` and \`${messageContext.prefix}log deleted\` instead.`
        ].join('\n'));
    }
}

export = AddLogChannelCommand;
