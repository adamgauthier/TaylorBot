'use strict';

const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const Command = require('../Command.js');
const TimeUtil = require('../../modules/TimeUtil.js');

const ArrayUtil = require('../../modules/ArrayUtil.js');
const PageMessage = require('../../modules/paging/PageMessage.js');
const EmbedDescriptionPageEditor = require('../../modules/paging/editors/EmbedDescriptionPageEditor.js');

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

        const lines = guildNames.map(gn => `${TimeUtil.formatSmall(gn.changed_at.getTime())} : ${gn.guild_name}`);
        const chunks = ArrayUtil.chunk(lines, 15);

        return new PageMessage(
            client,
            author,
            chunks.map(chunk => chunk.join('\n')),
            new EmbedDescriptionPageEditor(embed)
        ).send(channel);
    }
}

module.exports = ServerNamesCommand;