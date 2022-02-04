import { Command } from '../Command';
import { CommandMessageContext } from '../CommandMessageContext';

class AddSupporterServerCommand extends Command {
    constructor() {
        super({
            name: 'addsupporterserver',
            aliases: ['ass'],
            group: 'support',
            description: 'This command has been removed. Please use the `plus add` command instead.',
            examples: [''],
            guildOnly: true,

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

    async run({ message, client, messageContext }: CommandMessageContext): Promise<void> {
        await client.sendEmbedError(message.channel, [
            `This command has been removed.`,
            `Please use \`${messageContext.prefix}plus add\` instead.`
        ].join('\n'));
    }
}

export = AddSupporterServerCommand;
