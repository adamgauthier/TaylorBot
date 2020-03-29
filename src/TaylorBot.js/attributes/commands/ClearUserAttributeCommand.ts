import Command = require('../../commands/Command.js');
import { TaylorBotClient } from '../../client/TaylorBotClient.js';
import { Message } from 'discord.js';
import { SettableUserAttribute } from '../SettableUserAttribute.js';

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

    async run(commandContext: { client: TaylorBotClient; message: Message }): Promise<Message> {
        const { client, message } = commandContext;
        return client.sendEmbed(
            message.channel,
            await this.#attribute.clearCommand(commandContext)
        );
    }
}
