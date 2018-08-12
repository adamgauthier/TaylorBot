'use strict';

const { Paths } = require('globalobjects');

const DiscordEmbedFormatter = require(Paths.DiscordEmbedFormatter);
const Command = require('../../structures/Command.js');

class AvatarCommand extends Command {
    constructor() {
        super({
            name: 'avatar',
            aliases: ['av', 'avi'],
            group: 'info',
            description: 'Gets the avatar of a user in the current server.',
            examples: ['avatar @Enchanted13#1989', 'avatar Enchanted13'],
            guildOnly: true,

            args: [
                {
                    key: 'member',
                    label: 'user',
                    type: 'member-or-author',
                    prompt: 'What user would you like to see the avatar of?'
                }
            ]
        });
    }

    run({ message, client }, { member }) {
        const { user } = member;
        const embed = DiscordEmbedFormatter
            .baseUserEmbed(user)
            .setImage(DiscordEmbedFormatter.getAvatarURL(user, 1024));
        return client.sendEmbed(message.channel, embed);
    }
}

module.exports = AvatarCommand;