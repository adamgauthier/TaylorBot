import { Command } from '../Command';
import { CommandMessageContext } from '../CommandMessageContext';

class ClearLastFmCommand extends Command {
    constructor() {
        super({
            name: `clearlastfm`,
            aliases: ['clearfm'],
            group: 'attributes',
            description: `This command has been removed. Please use \`lastfm clear\` instead.`,
            examples: [''],

            args: []
        });
    }

    async run(commandContext: CommandMessageContext): Promise<void> {
        await commandContext.client.sendEmbedError(
            commandContext.message.channel,
            `This command has been removed. Please use \`${commandContext.messageContext.prefix}lastfm clear\` instead.`
        );
    }
}

export = ClearLastFmCommand;
