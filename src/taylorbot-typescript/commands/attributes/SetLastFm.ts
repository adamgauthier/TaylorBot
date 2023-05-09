import { Command } from '../Command';
import { CommandMessageContext } from '../CommandMessageContext';

class SetLastFmCommand extends Command {
    constructor() {
        super({
            name: `setlastfm`,
            aliases: ['setfm'],
            group: 'attributes',
            description: `This command has been removed. Please use </lastfm set:922354806574678086> instead.`,
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

    async run(commandContext: CommandMessageContext): Promise<void> {
        await commandContext.client.sendEmbedError(
            commandContext.message.channel,
            `This command has been removed. Please use </lastfm set:922354806574678086> instead.`
        );
    }
}

export = SetLastFmCommand;
