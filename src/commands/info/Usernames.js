'use strict';

const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const Command = require('../../structures/Command.js');
const TimeUtil = require('../../modules/TimeUtil.js');

const ArrayUtil = require('../../modules/ArrayUtil.js');
const ArrayEmbedDescriptionPageMessage = require('../../modules/paging/ArrayEmbedDescriptionPageMessage.js');

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
        const { channel, author } = message;
        const usernames = await client.master.database.usernames.getHistory(member.user, 75);
        const embed = DiscordEmbedFormatter.baseUserEmbed(member.user);

        const lines = usernames.map(u => `${TimeUtil.formatSmall(u.changed_at)} : ${u.username}`);
        const chunks = ArrayUtil.chunk(lines, 15);

        return new ArrayEmbedDescriptionPageMessage(
            client,
            author,
            embed,
            chunks.map(chunk => chunk.join('\n'))
        ).send(channel);
    }
}

module.exports = UsernamesCommand;