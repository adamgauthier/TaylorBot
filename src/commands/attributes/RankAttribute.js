'use strict';

const Command = require('../Command.js');

class RankAttributeCommand extends Command {
    constructor() {
        super({
            name: 'rank',
            group: 'attributes',
            description: 'Gets the ranking of an attribute for the current server.',
            examples: ['joined'],
            guildOnly: true,

            args: [
                {
                    key: 'attribute',
                    label: 'attribute',
                    prompt: 'What would you like to see the ranking of?',
                    type: 'member-attribute'
                }
            ]
        });
    }

    async run(commandContext, { attribute }) {
        const { guild, channel } = commandContext.message;

        const pageMessage = await attribute.rankCommand(commandContext, guild);

        return pageMessage.send(channel);
    }
}

module.exports = RankAttributeCommand;