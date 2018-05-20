'use strict';

const { Paths } = require('globalobjects');

const DiscordEmbedFormatter = require(Paths.DiscordEmbedFormatter);
const Command = require(Paths.Command);

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