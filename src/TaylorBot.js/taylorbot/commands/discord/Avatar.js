'use strict';

const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const Command = require('../Command.js');

class AvatarCommand extends Command {
    constructor() {
        super({
            name: 'avatar',
            aliases: ['av', 'avi'],
            group: 'discord',
            description: 'Gets the avatar of a user.',
            examples: ['@Enchanted13#1989', 'Enchanted13'],

            args: [
                {
                    key: 'user',
                    label: 'user',
                    type: 'user-or-author',
                    prompt: 'What user would you like to see the avatar of?'
                }
            ]
        });
    }

    run({ message, client }, { user }) {
        const embed = DiscordEmbedFormatter
            .baseUserEmbed(user)
            .setImage(DiscordEmbedFormatter.getAvatarURL(user, 1024));
        return client.sendEmbed(message.channel, embed);
    }
}

module.exports = AvatarCommand;