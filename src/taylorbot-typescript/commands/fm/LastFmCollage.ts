import Command = require('../Command.js');
import { CommandMessageContext } from '../CommandMessageContext';

class LastFmCollageCommand extends Command {
    constructor() {
        super({
            name: 'lastfmcollage',
            aliases: ['fmcollage', 'fmc'],
            group: 'fm',
            description: 'This command is obsolete and will be removed in a future version. Please use `lastfm collage` instead.',
            examples: ['7d 3', 'overall 4'],

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
            `This command is obsolete and will be removed in a future version. Please use \`${messageContext.prefix}lastfm collage\` or \`${messageContext.prefix}fm c\` instead.`
        );
    }
}

export = LastFmCollageCommand;
