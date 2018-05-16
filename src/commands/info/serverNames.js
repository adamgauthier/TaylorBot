'use strict';

const { GlobalPaths } = require('globalobjects');

const DiscordEmbedFormatter = require(GlobalPaths.DiscordEmbedFormatter);
const Command = require(GlobalPaths.Command);
const TimeUtil = require(GlobalPaths.TimeUtil);

class ServerNamesCommand extends Command {
    constructor() {
        super({
            name: 'servernames',
            aliases: ['snames', 'guildnames', 'gnames'],
            group: 'info',
            memberName: 'servernames',
            description: 'Gets a list of previous names for a server.',
            examples: ['servernames'],
            argsPromptLimit: 0,

            args: [
                {
                    key: 'guild',
                    label: 'server',
                    prompt: 'What server would you like to see the names of?',
                    type: 'guild-or-current',
                    error: 'Could not find server'
                }
            ]
        });
    }

    async run({ message, client }, { guild }) {
        const { channel } = message;
        const guildNames = await client.master.database.guildNames.getHistory(guild, 10);
        const embed = DiscordEmbedFormatter
            .baseGuildHeader(guild)
            .setDescription(
                guildNames.map(gn => `${TimeUtil.formatSmall(gn.changed_at)} : ${gn.guild_name}`).join('\n')
            );
        return client.sendEmbed(channel, embed);
    }
}

module.exports = ServerNamesCommand;