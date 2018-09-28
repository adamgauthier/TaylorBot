'use strict';

const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const Command = require('../Command.js');
const TimeUtil = require('../../modules/TimeUtil.js');

const ArrayUtil = require('../../modules/ArrayUtil.js');
const ArrayEmbedDescriptionPageMessage = require('../../modules/paging/ArrayEmbedDescriptionPageMessage.js');

class ServerNamesCommand extends Command {
    constructor() {
        super({
            name: 'servernames',
            aliases: ['snames', 'guildnames', 'gnames'],
            group: 'info',
            description: 'Gets a list of previous names for a server.',
            examples: [''],

            args: [
                {
                    key: 'guild',
                    label: 'server',
                    prompt: 'What server would you like to see the names of?',
                    type: 'guild-or-current'
                }
            ]
        });
    }

    async run({ message, client }, { guild }) {
        const { channel, author } = message;
        const guildNames = await client.master.database.guildNames.getHistory(guild, 75);
        const embed = DiscordEmbedFormatter.baseGuildHeader(guild);

        const lines = guildNames.map(gn => `${TimeUtil.formatSmall(gn.changed_at)} : ${gn.guild_name}`);
        const chunks = ArrayUtil.chunk(lines, 15);

        return new ArrayEmbedDescriptionPageMessage(
            client,
            author,
            chunks.map(chunk => chunk.join('\n')),
            embed
        ).send(channel);
    }
}

module.exports = ServerNamesCommand;