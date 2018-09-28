'use strict';

const Command = require('../commands/Command.js');

class SetUserAttributeCommand extends Command {
    constructor(attribute) {
        super({
            name: `set${attribute.id}`,
            aliases: attribute.aliases.map(a => `set${a}`),
            group: 'attributes',
            description: `Sets your ${attribute.description}.`,
            examples: [attribute.value.example],

            args: [
                {
                    key: 'value',
                    label: attribute.value.label,
                    type: attribute.value.type,
                    prompt: `What do you want to set your ${attribute.description} to?`
                }
            ]
        });
        this.attribute = attribute;
    }

    async run(commandContext, { value }) {
        const { client, message } = commandContext;
        return client.sendEmbed(
            message.channel,
            await this.attribute.set(commandContext, value)
        );
    }
}

module.exports = SetUserAttributeCommand;