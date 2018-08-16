'use strict';

const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const Command = require('../../structures/Command.js');

class ServerInfoCommand extends Command {
    constructor() {
        super({
            name: 'serverinfo',
            aliases: ['sinfo', 'guildinfo', 'ginfo'],
            group: 'info',
            description: 'Gets information about a server.',
            examples: ['sinfo'],

            args: [
                {
                    key: 'guild',
                    label: 'server',
                    prompt: 'What server would you like to see the info of?',
                    type: 'guild-or-current'
                }
            ]
        });
    }

    run({ message, client }, { guild }) {
        return client.sendEmbed(message.channel, DiscordEmbedFormatter.guild(guild));
    }
}

module.exports = ServerInfoCommand;