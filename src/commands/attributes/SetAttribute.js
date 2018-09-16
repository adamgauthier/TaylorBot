'use strict';

const Command = require('../../structures/Command.js');
const CommandsWatcher = require('../../watchers/Commands.js');

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
                    type: 'user-attribute',
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

    async run(commandContext, { attribute, value }) {
        const { registry } = commandContext.client.master;

        const cachedCommand = registry.commands.resolve(`set${attribute.id}`);

        registry.users.updateLastAnswered(commandContext.message.author, Date.now());
        return CommandsWatcher.runCommand(commandContext, cachedCommand, ` ${value}`);
    }
}

module.exports = SetAttributeCommand;