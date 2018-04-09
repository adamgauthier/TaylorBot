'use strict';

const { GlobalPaths } = require('globalobjects');

const ArgumentInfos = require(GlobalPaths.ArgumentInfos);
const DiscordEmbedFormatter = require(GlobalPaths.DiscordEmbedFormatter);
const Command = require(GlobalPaths.Command);

class JoinedCommand extends Command {
    constructor(client) {
        super(client, {
            name: 'avatar',
            group: 'info',
            memberName: 'avatar',
            description: 'Gets the avatar of a user in the current server.',
            examples: ['avatar @Enchanted13#1989', 'avatar Enchanted13'],
            guildOnly: true,
            argsPromptLimit: 0,

            args: [
                {
                    key: 'member',
                    label: 'user',
                    ...ArgumentInfos.MemberOrAuthor
                }
            ]
        });
    }

    run(message, { member }) {
        const { user } = member;
        const embed = DiscordEmbedFormatter
            .baseUserHeader(user)
            .setImage(DiscordEmbedFormatter.getAvatarURL(user, 1024));
        return this.client.sendEmbed(message.channel, embed);
    }
}

module.exports = JoinedCommand;