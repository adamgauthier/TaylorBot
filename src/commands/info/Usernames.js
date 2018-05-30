'use strict';

const { Paths } = require('globalobjects');

const DiscordEmbedFormatter = require(Paths.DiscordEmbedFormatter);
const Command = require(Paths.Command);
const TimeUtil = require(Paths.TimeUtil);

class UsernamesCommand extends Command {
    constructor() {
        super({
            name: 'usernames',
            aliases: ['names'],
            group: 'info',
            description: 'Gets a list of previous usernames for a user in the current server.',
            examples: ['usernames @Enchanted13#1989', 'names Enchanted13'],
            guildOnly: true,

            args: [
                {
                    key: 'member',
                    label: 'user',
                    type: 'member-or-author',
                    prompt: 'What user would you like to see the usernames history of?'
                }
            ]
        });
    }

    async run({ message, client }, { member }) {
        const usernames = await client.master.database.usernames.getHistory(member.user, 10);
        const embed = DiscordEmbedFormatter
            .baseUserHeader(member.user)
            .setDescription(
                usernames.map(u => `${TimeUtil.formatSmall(u.changed_at)} : ${u.username}`).join('\n')
            );
        return client.sendEmbed(message.channel, embed);
    }
}

module.exports = UsernamesCommand;