'use strict';

const Command = require('../Command.js');
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

    async run(commandContext, { attribute, value }) {
        const { registry } = commandContext.client.master;

        const cachedCommand = registry.commands.resolve(`set${attribute.id}`);

        await registry.answeredCooldowns.setAnswered(commandContext.message.author);
        return CommandsWatcher.runCommand(commandContext.messageContext, cachedCommand, ` ${value}`);
    }
}

module.exports = SetAttributeCommand;