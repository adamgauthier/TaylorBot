'use strict';

const { GlobalPaths } = require('globalobjects');

const DiscordEmbedFormatter = require(GlobalPaths.DiscordEmbedFormatter);
const Command = require(GlobalPaths.Command);

class ServerInfoCommand extends Command {
    constructor(client) {
        super(client, {
            name: 'serverinfo',
            aliases: ['sinfo', 'guildinfo', 'ginfo'],
            group: 'info',
            memberName: 'serverinfo',
            description: 'Gets information about a server.',
            examples: ['sinfo'],
            argsPromptLimit: 0,

            args: [
                {
                    key: 'guild',
                    label: 'server',
                    prompt: 'What server would you like to see the info of?',
                    type: 'guild-or-current',
                    error: 'Could not find server'
                }
            ]
        });
    }

    run(message, { guild }) {
        return this.client.sendEmbed(message.channel, DiscordEmbedFormatter.guild(guild));
    }
}

module.exports = ServerInfoCommand;