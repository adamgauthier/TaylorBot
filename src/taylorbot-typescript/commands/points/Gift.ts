import { Command } from '../Command';
import { DiscordEmbedFormatter } from '../../modules/discord/DiscordEmbedFormatter';
import { StringUtil } from '../../modules/util/StringUtil';
import { CommandMessageContext } from '../CommandMessageContext';
import { TaypointAmount } from '../../modules/points/TaypointAmount';
import { User } from 'discord.js';

class GiftCommand extends Command {
    constructor() {
        super({
            name: 'gift',
            aliases: ['give'],
            group: 'Points 💰',
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

    async run({ message, client, author }: CommandMessageContext, { amount, users }: { amount: TaypointAmount; users: User[] }): Promise<void> {
        const { channel } = message;
        const {
            usersToGift, gifted_count, original_count
        } = await client.master.database.users.transferTaypointCount(author, users, amount);

        await client.sendEmbed(channel, DiscordEmbedFormatter
            .baseUserEmbed(author)
            .setDescription([
                `Successfully gifted ${StringUtil.plural(gifted_count, 'taypoint', '**')} out of ${StringUtil.formatNumberString(original_count)}:`,
                ...usersToGift.map(({ user, giftedCount }) => `${StringUtil.plural(giftedCount, 'taypoint', '**')} to ${user}`)
            ].join('\n'))
        );
    }
}

export = GiftCommand;
