import moment = require('moment');

import DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
import CommandMessageContext = require('../../commands/CommandMessageContext.js');
import { UserAttributePresenter } from '../UserAttributePresenter.js';
import { UserAttribute } from '../UserAttribute.js';
import { MessageEmbed, User } from 'discord.js';

export class AgePresenter implements UserAttributePresenter {
    constructor(_: UserAttribute) { }

    present(commandContext: CommandMessageContext, user: User, { age, birthday }: Record<string, any> & { rank: string }): Promise<MessageEmbed> {
        const embed = DiscordEmbedFormatter.baseUserEmbed(user);

        if (!birthday) {
            const setCommand = commandContext.client.master.registry.commands.resolve(`setbirthday`);
            const context = new CommandMessageContext(commandContext.messageContext, setCommand);

            return Promise.resolve(
                embed
                    .setColor('#f04747')
                    .setDescription([
                        `${user.username} is **${age.integer_value}** years old.`,
                        `⚠ This user has set their age manually so it might be outdated. ⚠`,
                        `Please use \`${context.usage()}\`, your age will automatically update and you will get points on your birthday every year! 🎈`,
                        `If you don't want to share your exact birthday but still want the points as well as horoscope and age commands, use \`setprivatebirthday\` in DMs. 🕵️‍`
                    ].join('\n'))
            );
        }

        const parsedBirthday = moment.utc(birthday.birthday, 'YYYY-MM-DD');

        if (parsedBirthday.year() === 1804) {
            const setCommand = commandContext.client.master.registry.commands.resolve(`setbirthday`);
            const context = new CommandMessageContext(commandContext.messageContext, setCommand);

            return Promise.resolve(
                embed
                    .setColor('#f04747')
                    .setDescription([
                        `I don't know how old ${user.username} is because their birthday was set without a year. 😕`,
                        `Please use \`${context.usage()}\`, your age will automatically update and you will get points on your birthday every year! 🎈`,
                        `If you don't want to share your exact birthday but still want the points as well as horoscope and age commands, use \`setprivatebirthday\` in DMs. 🕵️‍`
                    ].join('\n'))
            );
        }

        const computedAge = moment.utc().diff(parsedBirthday, 'years');

        return Promise.resolve(
            embed.setDescription(`${user.username} is **${computedAge}** years old.`)
        );
    }
}
