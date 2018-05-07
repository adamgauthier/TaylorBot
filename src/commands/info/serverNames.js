'use strict';

const { GlobalPaths } = require('globalobjects');

const DiscordEmbedFormatter = require(GlobalPaths.DiscordEmbedFormatter);
const Command = require(GlobalPaths.Command);
const TimeUtil = require(GlobalPaths.TimeUtil);

class ServerNamesCommand extends Command {
    constructor(client) {
        super(client, {
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

    async run(message, { guild }) {
        const guildNames = await this.client.master.database.guildNames.getHistory(guild, 10);
        const embed = DiscordEmbedFormatter
            .baseGuildHeader(guild)
            .setDescription(
                guildNames.map(gn => `${TimeUtil.formatSmall(gn.changed_at)} : ${gn.guild_name}`).join('\n')
            );
        return this.client.sendEmbed(message.channel, embed);
    }
}

module.exports = ServerNamesCommand;