import Command = require('../../commands/Command.js');
import { Message } from 'discord.js';
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

    async run(commandContext: CommandMessageContext): Promise<Message> {
        const { client, message } = commandContext;
        return client.sendEmbed(
            message.channel,
            await this.#attribute.clearCommand(commandContext)
        );
    }
}
