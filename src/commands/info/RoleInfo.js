'use strict';

const { Paths } = require('globalobjects');

const DiscordEmbedFormatter = require(Paths.DiscordEmbedFormatter);
const Command = require(Paths.Command);

class RoleInfoCommand extends Command {
    constructor() {
        super({
            name: 'roleinfo',
            aliases: ['rinfo'],
            group: 'info',
            description: 'Gets information about a role.',
            examples: ['roleinfo mods', 'rinfo'],

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