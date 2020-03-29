import Command = require('../../commands/Command');
import { Message, User } from 'discord.js';
import { UserAttribute } from '../UserAttribute';
import CommandMessageContext = require('../../commands/CommandMessageContext');

export class GetUserAttributeCommand extends Command {
    readonly #attribute: UserAttribute;

    constructor(attribute: UserAttribute) {
        super({
            name: attribute.id,
            aliases: attribute.aliases,
            group: 'attributes',
            description: `Gets the ${attribute.description} of a user.`,
            examples: ['@Enchanted13#1989'],

            args: [
                {
                    key: 'user',
                    label: 'user',
                    type: 'user-or-author',
                    prompt: `What user would you like to see the ${attribute.description} of?`
                }
            ]
        });
        this.#attribute = attribute;
    }

    async run(commandContext: CommandMessageContext, { user }: { user: User }): Promise<Message> {
        const { client, message } = commandContext;
        return client.sendEmbed(
            message.channel,
            await this.#attribute.getCommand(commandContext, user)
        );
    }
}
