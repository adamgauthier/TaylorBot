import { Command } from '../Command';
import { CommandMessageContext } from '../CommandMessageContext';
import { SettableUserAttribute } from '../../attributes/SettableUserAttribute';

class ClearAttributeCommand extends Command {
    constructor() {
        super({
            name: 'clear',
            aliases: ['clearattribute', 'ca'],
            group: 'attributes',
            description: 'Clears one of your attributes.',
            examples: ['bae'],

            args: [
                {
                    key: 'attribute',
                    label: 'attribute',
                    type: 'settable-attribute',
                    prompt: 'What attribute do you want to clear?'
                }
            ]
        });
    }

    async run(commandContext: CommandMessageContext, { attribute }: { attribute: SettableUserAttribute }): Promise<void> {
        const { client, message } = commandContext;

        await client.sendEmbed(
            message.channel,
            await attribute.clearCommand(commandContext)
        );
    }
}

export = ClearAttributeCommand;
