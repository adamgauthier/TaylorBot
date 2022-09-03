import { Command } from '../Command';
import { CommandMessageContext } from '../CommandMessageContext';

class LastFmCollageCommand extends Command {
    constructor() {
        super({
            name: 'lastfmcollage',
            aliases: ['fmcollage', 'fmc'],
            group: 'fm',
            description: 'This command has been removed. Please use </lastfm collage:922354806574678086> instead.',
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
        await client.sendEmbedError(
            message.channel,
            `This command has been removed. Please use </lastfm collage:922354806574678086> instead.`
        );
    }
}

export = LastFmCollageCommand;
