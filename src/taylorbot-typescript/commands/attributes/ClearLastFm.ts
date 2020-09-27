import { Command } from '../Command';
import { CommandMessageContext } from '../CommandMessageContext';

class ClearLastFmCommand extends Command {
    constructor() {
        super({
            name: `clearlastfm`,
            aliases: ['clearfm'],
            group: 'attributes',
            description: `This command is obsolete and will be removed in a future version. Please use \`lastfm clear\` instead.`,
            examples: [''],

            args: []
        });
    }

    async run(commandContext: CommandMessageContext): Promise<void> {
        await commandContext.client.sendEmbedError(
            commandContext.message.channel,
            `This command is obsolete and will be removed in a future version. Please use \`${commandContext.messageContext.prefix}lastfm clear\` instead.`
        );
    }
}

export = ClearLastFmCommand;
