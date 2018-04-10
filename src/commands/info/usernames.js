'use strict';

const { GlobalPaths } = require('globalobjects');

const ArgumentInfos = require(GlobalPaths.ArgumentInfos);
const DiscordEmbedFormatter = require(GlobalPaths.DiscordEmbedFormatter);
const Command = require(GlobalPaths.Command);
const TimeUtil = require(GlobalPaths.TimeUtil);

class UsernamesCommand extends Command {
    constructor(client) {
        super(client, {
            name: 'usernames',
            aliases: ['names', 'n'],
            group: 'info',
            memberName: 'usernames',
            description: 'Gets a list of previous usernames for a user in the current server.',
            examples: ['usernames @Enchanted13#1989', 'names Enchanted13'],
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

    async run(message, { member }) {
        const usernames = await this.client.database.getUsernameHistory(member.user, 10);
        const embed = DiscordEmbedFormatter
            .baseUserHeader(member.user)
            .setDescription(
                usernames.map(u => `${TimeUtil.formatSmall(u.changed_at)} : ${u.username}`).join('\n')
            );
        return this.client.sendEmbed(message.channel, embed);
    }
}

module.exports = UsernamesCommand;