'use strict';

const Command = require('../Command.js');

class ListAttributeCommand extends Command {
    constructor() {
        super({
            name: 'list',
            group: 'attributes',
            description: 'Gets the list of an attribute of users in the current server.',
            examples: ['lastfm'],
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

    async run(commandContext, { attribute }) {
        const { guild, channel } = commandContext.message;

        const pageMessage = await attribute.listCommand(commandContext, guild);

        return pageMessage.send(channel);
    }
}

module.exports = ListAttributeCommand;