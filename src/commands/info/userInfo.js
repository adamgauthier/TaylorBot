'use strict';

const { GlobalPaths } = require('globalobjects');

const ArgumentInfos = require(GlobalPaths.ArgumentInfos);
const DiscordEmbedFormatter = require(GlobalPaths.DiscordEmbedFormatter);
const Command = require(GlobalPaths.Command);

class UserInfoCommand extends Command {
    constructor() {
        super({
            name: 'userinfo',
            aliases: ['uinfo'],
            group: 'info',
            memberName: 'userinfo',
            description: 'Gets information about a user.',
            examples: ['uinfo @Enchanted13#1989', 'uinfo Enchanted13'],
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

    run({ message, client }, { member }) {
        return client.sendEmbed(message.channel, DiscordEmbedFormatter.member(member));
    }
}

module.exports = UserInfoCommand;