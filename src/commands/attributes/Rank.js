'use strict';

const Command = require('../../structures/Command.js');

class RankCommand extends Command {
    constructor() {
        super({
            name: 'rank',
            group: 'attributes',
            description: 'Gets the ranking of an attribute for the current server.',
            examples: ['rank joined'],
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

        const pageMessage = await attribute.rank(commandContext, guild);

        return pageMessage.send(channel);
    }
}

module.exports = RankCommand;