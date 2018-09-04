'use strict';

const Command = require('../structures/Command.js');

class ClearUserAttributeCommand extends Command {
    constructor(attribute) {
        super({
            name: `clear${attribute.id}`,
            group: 'attributes',
            description: `Clears your ${attribute.description}.`,
            examples: [`clear${attribute.id}`],

            args: []
        });
        this.attribute = attribute;
    }

    async run(commandContext) {
        const { client, message } = commandContext;
        return client.sendEmbed(
            message.channel,
            await this.attribute.clear(commandContext)
        );
    }
}

module.exports = ClearUserAttributeCommand;