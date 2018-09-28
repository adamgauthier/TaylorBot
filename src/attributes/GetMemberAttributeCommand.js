'use strict';

const Command = require('../commands/Command.js');

class GetMemberAttributeCommand extends Command {
    constructor(attribute) {
        super({
            name: attribute.id,
            aliases: attribute.aliases,
            group: 'attributes',
            description: `Gets the ${attribute.description} of a user in the current server.`,
            examples: ['@Enchanted13#1989'],
            guildOnly: true,

            args: [
                {
                    key: 'member',
                    label: 'user',
                    type: 'member-or-author',
                    prompt: `What user would you like to see the ${attribute.description} of?`
                }
            ]
        });
        this.attribute = attribute;
    }

    async run(commandContext, { member }) {
        const { client, message } = commandContext;
        return client.sendEmbed(
            message.channel,
            await this.attribute.retrieve(commandContext, member)
        );
    }
}

module.exports = GetMemberAttributeCommand;