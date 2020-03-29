import Command = require('../../commands/Command');
import CommandMessageContext = require('../../commands/CommandMessageContext');
import { TaylorBotClient } from '../../client/TaylorBotClient';
import { Message } from 'discord.js';
import { SettableUserAttribute } from '../SettableUserAttribute';

export class SetUserAttributeCommand extends Command {
    #attribute: SettableUserAttribute;

    constructor(attribute: SettableUserAttribute) {
        super({
            name: `set${attribute.id}`,
            aliases: attribute.aliases.map((a: string) => `set${a}`),
            group: 'attributes',
            description: `Sets your ${attribute.description}.`,
            examples: [attribute.value.example],
            maxDailyUseCount: attribute.value.maxDailySetCount === undefined ? null : attribute.value.maxDailySetCount,

            args: [
                {
                    key: 'value',
                    label: attribute.value.label,
                    type: attribute.value.type,
                    prompt: `What do you want to set your ${attribute.description} to?`
                }
            ]
        });
        this.#attribute = attribute;
    }

    async run(commandContext: CommandMessageContext, { value }: { value: any }): Promise<Message> {
        const { client, message }: { client: TaylorBotClient; message: Message } = commandContext;
        return client.sendEmbed(
            message.channel,
            await this.#attribute.setCommand(commandContext, value)
        );
    }
}
