import moment = require('moment');

import { DiscordEmbedFormatter } from '../../modules/discord/DiscordEmbedFormatter';
import { UserAttributePresenter } from '../UserAttributePresenter.js';
import { EmbedBuilder, User } from 'discord.js';
import { CommandMessageContext } from '../../commands/CommandMessageContext';

export class AgePresenter implements UserAttributePresenter {
    present(commandContext: CommandMessageContext, user: User, { age, birthday }: Record<string, any> & { rank: string }): Promise<EmbedBuilder> {
        const embed = DiscordEmbedFormatter.baseUserSuccessEmbed(user);

        if (!birthday) {
            return Promise.resolve(
                embed
                    .setColor('#f04747')
                    .setDescription([
                        `${user.username} is **${age.integer_value}** years old.`,
                        `⚠ This user has set their age manually so it might be outdated. ⚠`,
                        `Please use </birthday set:1016938623880400907> with the **year** option, your age will automatically update and you will get points on your birthday every year! 🎈`,
                        `If you don't want to share your exact birthday, but want the points, horoscope and age commands, use </birthday set:1016938623880400907> with the **privately** option. 🕵️‍`
                    ].join('\n'))
            );
        }

        const parsedBirthday = moment.utc(birthday.birthday, 'YYYY-MM-DD');

        if (parsedBirthday.year() === 1804) {
            return Promise.resolve(
                embed
                    .setColor('#f04747')
                    .setDescription([
                        `I don't know how old ${user.username} is because their birthday was set without a year. 😕`,
                        `Please use </birthday set:1016938623880400907> with the **year** option, your age will automatically update and you will get points on your birthday every year! 🎈`,
                        `If you don't want to share your exact birthday, but want the points, horoscope and age commands, use </birthday set:1016938623880400907> with the **privately** option. 🕵️‍`
                    ].join('\n'))
            );
        }

        const computedAge = moment.utc().diff(parsedBirthday, 'years');

        return Promise.resolve(
            embed.setDescription(`${user.username} is **${computedAge}** years old.`)
        );
    }
}
