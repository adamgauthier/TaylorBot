'use strict';

const Command = require('../structures/Command.js');

class GetUserAttributeCommand extends Command {
    constructor(attribute) {
        super({
            name: attribute.id,
            group: 'attributes',
            description: `Gets the ${attribute.description} of a user.`,
            examples: [`${attribute.id} @Enchanted13#1989`],

            args: [
                {
                    key: 'user',
                    label: 'user',
                    type: 'user-or-author',
                    prompt: `What user would you like to see the ${attribute.description} of?`
                }
            ]
        });
        this.attribute = attribute;
    }

    async run(commandContext, { user }) {
        const { client, message } = commandContext;
        return client.sendEmbed(
            message.channel,
            await this.attribute.retrieve(commandContext, user)
        );
    }
}

module.exports = GetUserAttributeCommand;