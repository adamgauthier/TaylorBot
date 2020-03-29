import Command = require('../../commands/Command');
import { Attribute } from '../Attribute';
import CommandMessageContext = require('../../commands/CommandMessageContext');

export class ListAttributeCommand extends Command {
    readonly #attribute: Attribute;
    constructor(attribute: Attribute) {
        super({
            name: `list${attribute.id}`,
            aliases: attribute.aliases.map(a => `list${a}`),
            group: 'attributes',
            description: `Gets the list of the ${attribute.description} of users in the current server.`,
            examples: [''],
            guildOnly: true,

            args: []
        });
        this.#attribute = attribute;
    }

    async run(commandContext: CommandMessageContext): Promise<void> {
        const { guild, channel } = commandContext.message;

        const pageMessage = await this.#attribute.listCommand(commandContext, guild);

        await pageMessage.send(channel);
    }
}
