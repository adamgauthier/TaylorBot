import { Command } from '../../commands/Command';
import { SettableUserAttribute } from '../SettableUserAttribute';
import { CommandMessageContext } from '../../commands/CommandMessageContext';

export class ClearUserAttributeCommand extends Command {
    readonly #attribute: SettableUserAttribute;

    constructor(attribute: SettableUserAttribute) {
        super({
            name: `clear${attribute.id}`,
            aliases: attribute.aliases.map((a: string) => `clear${a}`),
            group: 'attributes',
            description: `Clears your ${attribute.description}.`,
            examples: [''],

            args: []
        });
        this.#attribute = attribute;
    }

    async run(commandContext: CommandMessageContext): Promise<void> {
        const { client, message } = commandContext;
        await client.sendEmbed(
            message.channel,
            await this.#attribute.clearCommand(commandContext)
        );
    }
}
