import { Command } from '../Command';
import { CommandMessageContext } from '../CommandMessageContext';
import { Attribute } from '../../attributes/Attribute';

class ListAttributeCommand extends Command {
    constructor() {
        super({
            name: 'list',
            group: 'attributes',
            description: 'Gets the list of an attribute of users in the current server.',
            examples: ['birthday'],
            guildOnly: true,

            args: [
                {
                    key: 'attribute',
                    label: 'attribute',
                    type: 'listable-attribute',
                    prompt: 'What attribute would you like to see listed?'
                }
            ]
        });
    }

    async run(commandContext: CommandMessageContext, { attribute }: { attribute: Attribute }): Promise<void> {
        const { guild, channel } = commandContext.message;

        const pageMessage = await attribute.listCommand(commandContext, guild!);

        await pageMessage.send(channel);
    }
}

export = ListAttributeCommand;
