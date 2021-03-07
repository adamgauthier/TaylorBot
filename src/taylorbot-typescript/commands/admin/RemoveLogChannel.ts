import { Command } from '../Command';
import { CommandMessageContext } from '../CommandMessageContext';

class RemoveLogChannelCommand extends Command {
    constructor() {
        super({
            name: 'removelogchannel',
            aliases: ['rlc'],
            group: 'admin',
            description: 'This command is obsolete and will be removed in a future version. Please use the `log member stop` or `log deleted stop` commands instead.',
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
            `This command is obsolete and will be removed in a future version.`,
            `Please use \`${messageContext.prefix}log member stop\` and \`${messageContext.prefix}log deleted stop\` instead.`
        ].join('\n'));
    }
}

export = RemoveLogChannelCommand;
