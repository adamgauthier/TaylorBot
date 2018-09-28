'use strict';

const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const Command = require('../Command.js');

class RoleInfoCommand extends Command {
    constructor() {
        super({
            name: 'roleinfo',
            aliases: ['rinfo'],
            group: 'info',
            description: 'Gets information about a role.',
            examples: ['mods', ''],

            args: [
                {
                    key: 'role',
                    label: 'role',
                    prompt: 'What role would you like to see the info of?',
                    type: 'role-or-owned'
                }
            ]
        });
    }

    run({ message, client }, { role }) {
        return client.sendEmbed(message.channel, DiscordEmbedFormatter.role(role));
    }
}

module.exports = RoleInfoCommand;