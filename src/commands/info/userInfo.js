'use strict';

const { GlobalPaths } = require('globalobjects');

const ArgumentInfos = require(GlobalPaths.ArgumentInfos);
const DiscordEmbedFormatter = require(GlobalPaths.DiscordEmbedFormatter);
const Command = require(GlobalPaths.Command);

class UserInfoCommand extends Command {
    constructor(client) {
        super(client, {
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

    run(message, { member }) {
        return this.client.sendEmbed(message.channel, DiscordEmbedFormatter.member(member));
    }
}

module.exports = UserInfoCommand;