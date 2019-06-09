'use strict';

const Command = require('../Command.js');
const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const StringUtil = require('../../modules/StringUtil.js');

class GiftCommand extends Command {
    constructor() {
        super({
            name: 'gift',
            aliases: ['give'],
            group: 'points',
            description: 'Gifts a specified amount of taypoints to pinged users.',
            examples: ['13 @Enchanted13#1989', 'all @Enchanted13#1989'],

            args: [
                {
                    key: 'amount',
                    label: 'taypoints',
                    type: 'taypoint-amount',
                    prompt: 'How much of your taypoints do you want to gift?'
                },
                {
                    key: 'users',
                    label: 'users',
                    type: 'mentioned-users-not-author',
                    prompt: 'What users would you like to gift taypoints to (must be mentioned)?'
                }
            ]
        });
    }

    async run({ message, client }, { amount, users }) {
        const { author, channel } = message;
        const {
            usersToGift, gifted_count, original_count
        } = await client.master.database.users.transferTaypointCount(author, users, amount);

        return client.sendEmbed(channel, DiscordEmbedFormatter
            .baseUserEmbed(author)
            .setDescription([
                `Successfully gifted ${StringUtil.plural(gifted_count, 'taypoint', '**')} out of ${StringUtil.formatNumberString(original_count)}:`,
                ...usersToGift.map(({ user, giftedCount }) => `${StringUtil.plural(giftedCount, 'taypoint', '**')} to ${user}`)
            ].join('\n'))
        );
    }
}

module.exports = GiftCommand;
