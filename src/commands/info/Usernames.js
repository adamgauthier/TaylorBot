'use strict';

const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const Command = require('../Command.js');
const TimeUtil = require('../../modules/TimeUtil.js');
const ArrayUtil = require('../../modules/ArrayUtil.js');
const PageMessage = require('../../modules/paging/PageMessage.js');
const EmbedDescriptionPageEditor = require('../../modules/paging/editors/EmbedDescriptionPageEditor.js');

class UsernamesCommand extends Command {
    constructor() {
        super({
            name: 'usernames',
            aliases: ['names'],
            group: 'info',
            description: 'Gets a list of previous usernames for a user.',
            examples: ['@Enchanted13#1989', 'Enchanted13'],

            args: [
                {
                    key: 'user',
                    label: 'user',
                    type: 'user-or-author',
                    prompt: 'What user would you like to see the usernames history of?'
                }
            ]
        });
    }

    async run({ message, client }, { user }) {
        const { channel, author } = message;
        const usernames = await client.master.database.usernames.getHistory(user, 75);
        const embed = DiscordEmbedFormatter.baseUserEmbed(user);

        const lines = usernames.map(u => `${TimeUtil.formatSmall(u.changed_at.getTime())} : ${u.username}`);
        const chunks = ArrayUtil.chunk(lines, 15);

        return new PageMessage(
            client,
            author,
            chunks.map(chunk => chunk.join('\n')),
            new EmbedDescriptionPageEditor(embed)
        ).send(channel);
    }
}

module.exports = UsernamesCommand;