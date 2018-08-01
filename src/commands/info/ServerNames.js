'use strict';

const { Paths } = require('globalobjects');

const DiscordEmbedFormatter = require(Paths.DiscordEmbedFormatter);
const Command = require(Paths.Command);
const TimeUtil = require(Paths.TimeUtil);

const ArrayUtil = require('../../modules/ArrayUtil.js');
const ArrayDescriptionPageMessage = require('../../modules/paging/ArrayDescriptionPageMessage.js');

class ServerNamesCommand extends Command {
    constructor() {
        super({
            name: 'servernames',
            aliases: ['snames', 'guildnames', 'gnames'],
            group: 'info',
            description: 'Gets a list of previous names for a server.',
            examples: ['servernames'],

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

        return new ArrayDescriptionPageMessage(
            client,
            author,
            embed,
            chunks.map(chunk => chunk.join('\n'))
        ).send(channel);
    }
}

module.exports = ServerNamesCommand;