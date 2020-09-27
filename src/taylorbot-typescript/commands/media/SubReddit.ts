import { Command } from '../Command';
import { CommandError } from '../CommandError';
import { RedditModule } from '../../modules/reddit/RedditModule';
import { DiscordEmbedFormatter } from '../../modules/discord/DiscordEmbedFormatter';
import { StringUtil } from '../../modules/util/StringUtil';
import { CommandMessageContext } from '../CommandMessageContext';

class SubRedditCommand extends Command {
    constructor() {
        super({
            name: 'subreddit',
            aliases: ['sub'],
            group: 'Media 📷',
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

    async run({ message, client, author }: CommandMessageContext, { subreddit }: { subreddit: string }): Promise<void> {
        const { channel } = message;

        const response = await RedditModule.getSubredditAbout(subreddit);

        if (response.error) {
            if (response.reason === 'quarantined') {
                throw new CommandError(`Can't get subreddit info for '${subreddit}' because it is quarantined.`);
            }
            throw new CommandError(`An error occurred when trying to get subreddit info for '${subreddit}'.`);
        }

        const subredditAbout = response.data;

        await client.sendEmbed(channel,
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

export = SubRedditCommand;
