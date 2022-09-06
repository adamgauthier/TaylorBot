import { Command } from '../Command';
import { CommandMessageContext } from '../CommandMessageContext';

class SetLastFmCommand extends Command {
    constructor() {
        super({
            name: `setlastfm`,
            aliases: ['setfm'],
            group: 'attributes',
            description: `This command has been removed. Please use \`lastfm set\` instead.`,
            examples: ['taylor'],

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

    async run(commandContext: CommandMessageContext, { lastFmUsername }: { lastFmUsername: string }): Promise<void> {
        await commandContext.client.sendEmbedError(
            commandContext.message.channel,
            `This command has been removed. Please use \`${commandContext.messageContext.prefix}lastfm set ${lastFmUsername}\` instead.`
        );
    }
}

export = SetLastFmCommand;
