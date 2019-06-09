'use strict';

const Command = require('../Command.js');
const CommandError = require('../CommandError.js');
const Reddit = require('../../modules/reddit/RedditModule.js');
const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const StringUtil = require('../../modules/StringUtil.js');

class SubRedditCommand extends Command {
    constructor() {
        super({
            name: 'subreddit',
            aliases: ['sub'],
            group: 'media',
            description: 'Gets info on a subreddit!',
            examples: ['taylorswift'],
            maxDailyUseCount: 100,

            args: [
                {
                    key: 'subreddit',
                    label: 'subreddit-name',
                    type: 'subreddit',
                    prompt: 'What is the name of the subreddit you want info about?'
                }
            ]
        });
    }

    async run({ message, client }, { subreddit }) {
        const { author, channel } = message;

        const response = await Reddit.getSubredditAbout(subreddit);

        if (response.error) {
            if (response.reason === 'quarantined') {
                throw new CommandError(`Can't get subreddit info for '${subreddit}' because it is quarantined.`);
            }
            throw new CommandError(`An error occurred when trying to get subreddit info for '${subreddit}'.`);
        }

        const subredditAbout = response.data;

        return client.sendEmbed(channel,
            DiscordEmbedFormatter
                .baseUserHeader(author)
                .setColor(subredditAbout.key_color)
                .setTitle(subredditAbout.title)
                .setURL(`https://www.reddit.com/${subredditAbout.display_name_prefixed}`)
                .setThumbnail(subredditAbout.icon_img)
                .setTimestamp(subredditAbout.created * 1000)
                .setDescription(subredditAbout.public_description)
                .addField('Users',
                    `${StringUtil.plural(subredditAbout.subscribers, 'subscriber', '`')} (\`${StringUtil.formatNumberString(subredditAbout.active_user_count)}\` online)`
                )
                .setFooter(subredditAbout.display_name_prefixed, 'https://i.imgur.com/HbUa6WQ.png')
        );
    }
}

module.exports = SubRedditCommand;
