'use strict';

const Command = require('../../structures/Command.js');

class GetMemberAttributeCommand extends Command {
    constructor() {
        super({
            name: 'getmemberattribute',
            aliases: ['gma'],
            group: 'attributes',
            description: 'Gets a member attribute for a user in the current server.',
            examples: ['joined @Enchanted13#1989', 'joined'],
            guildOnly: true,

            args: [
                {
                    key: 'attribute',
                    label: 'attribute',
                    type: 'member-attribute',
                    prompt: 'What attribute would you like to see from user?'
                },
                {
                    key: 'member',
                    label: 'user',
                    type: 'member-or-author',
                    prompt: 'What user would you like to see the attribute of?'
                }
            ]
        });
    }

    async run(commandContext, { attribute, member }) {
        const { client, message } = commandContext;
        return client.sendEmbed(
            message.channel,
            await attribute.retrieve(commandContext, member)
        );
    }
}

module.exports = GetMemberAttributeCommand;