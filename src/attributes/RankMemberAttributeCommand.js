'use strict';

const Command = require('../structures/Command.js');

class RankMemberAttributeCommand extends Command {
    constructor(attribute) {
        super({
            name: `rank${attribute.id}`,
            aliases: attribute.aliases.map(a => `rank${a}`),
            group: 'attributes',
            description: `Gets the ranking of the ${attribute.description} of users in the current server.`,
            examples: [''],
            guildOnly: true,

            args: []
        });
        this.attribute = attribute;
    }

    async run(commandContext) {
        const { guild, channel } = commandContext.message;

        const pageMessage = await this.attribute.rank(commandContext, guild);

        return pageMessage.send(channel);
    }
}

module.exports = RankMemberAttributeCommand;