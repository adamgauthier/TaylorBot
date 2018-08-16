'use strict';

const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const Command = require('../../structures/Command.js');

class UserInfoCommand extends Command {
    constructor() {
        super({
            name: 'userinfo',
            aliases: ['uinfo'],
            group: 'info',
            description: 'Gets information about a user.',
            examples: ['uinfo @Enchanted13#1989', 'uinfo Enchanted13'],
            guildOnly: true,

            args: [
                {
                    key: 'member',
                    label: 'user',
                    type: 'member-or-author',
                    prompt: 'What user would you like to see the info of?'
                }
            ]
        });
    }

    run({ message, client }, { member }) {
        return client.sendEmbed(message.channel, DiscordEmbedFormatter.member(member));
    }
}

module.exports = UserInfoCommand;