import moment = require('moment');

import { Command } from '../Command';
import { CommandError } from '../CommandError';
import { DiscordEmbedFormatter } from '../../modules/discord/DiscordEmbedFormatter';
import { HoroscopeModule } from '../../modules/horoscope/HoroscopeModule';
import { CommandMessageContext } from '../CommandMessageContext';
import { User } from 'discord.js';

class HoroscopeCommand extends Command {
    constructor() {
        super({
            name: 'horoscope',
            aliases: ['hs'],
            group: 'Knowledge ❓',
            description: 'Gets the horoscope of a user based on their set birthday.',
            examples: ['@Enchanted13#1989', 'Enchanted13'],
            maxDailyUseCount: 20,

            args: [
                {
                    key: 'user',
                    label: 'user',
                    type: 'user-or-author',
                    prompt: 'What user would you like to see the horoscope of?'
                }
            ]
        });
    }

    async run({ message, client }: CommandMessageContext, { user }: { user: User }): Promise<void> {
        const birthdayAttribute = await client.master.database.birthdays.getZodiac(user);
        if (!birthdayAttribute)
            throw new CommandError(`${user.username}'s birthday is not set. They can use the \`setbirthday\` command to set it.`);

        const { date, horoscope, sunsign } = await HoroscopeModule.getDailyHoroscope(birthdayAttribute.zodiac);

        await client.sendEmbed(message.channel, DiscordEmbedFormatter
            .baseUserEmbed(user)
            .setTitle(`${sunsign} - ${moment.utc(date, 'YYYY-MM-DD').format('dddd, MMMM Do YYYY')}`)
            .setDescription(horoscope)
        );
    }
}

export = HoroscopeCommand;
