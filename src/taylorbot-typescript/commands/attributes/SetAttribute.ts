import CommandsWatcher = require('../../watchers/Commands');
import { CommandMessageContext } from '../CommandMessageContext';
import { Command } from '../Command';
import { UserAttribute } from '../../attributes/UserAttribute';

class SetAttributeCommand extends Command {
    constructor() {
        super({
            name: 'set',
            aliases: ['setattribute', 'sa'],
            group: 'attributes',
            description: 'Sets one of your attributes to a value.',
            examples: ['bae Taylor Swift'],

            args: [
                {
                    key: 'attribute',
                    label: 'attribute',
                    type: 'settable-attribute',
                    prompt: 'What attribute do you want to set?'
                },
                {
                    key: 'value',
                    label: 'value',
                    type: 'multiline-text',
                    prompt: 'What value do you want to set it to?'
                }
            ]
        });
    }

    async run(commandContext: CommandMessageContext, { attribute, value }: { attribute: UserAttribute; value: string }): Promise<void> {
        const { registry } = commandContext.client.master;

        const cachedCommand = registry.commands.resolve(`set${attribute.id}`)!;

        await CommandsWatcher.runCommand(commandContext.messageContext, cachedCommand, ` ${value}`);
    }
}

export = SetAttributeCommand;
