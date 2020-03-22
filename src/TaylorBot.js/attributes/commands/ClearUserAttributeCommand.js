'use strict';

const Command = require('../../commands/Command.js');

class ClearUserAttributeCommand extends Command {
    constructor(attribute) {
        super({
            name: `clear${attribute.id}`,
            aliases: attribute.aliases.map(a => `clear${a}`),
            group: 'attributes',
            description: `Clears your ${attribute.description}.`,
            examples: [''],

            args: []
        });
        this.attribute = attribute;
    }

    async run(commandContext) {
        const { client, message } = commandContext;
        return client.sendEmbed(
            message.channel,
            await this.attribute.clearCommand(commandContext)
        );
    }
}

module.exports = ClearUserAttributeCommand;