import { User } from 'discord.js';
import moment = require('moment-timezone');

import DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
import { Command } from '../Command';
import { CommandMessageContext } from '../CommandMessageContext';

class ProfileCommand extends Command {
    constructor() {
        super({
            name: 'profile',
            aliases: ['info', 'asl'],
            group: 'Stats 📊',
            description: 'Gets the profile of a user.',
            examples: ['@Enchanted13#1989', 'Enchanted13'],

            args: [
                {
                    key: 'user',
                    label: 'user',
                    type: 'user-or-author',
                    prompt: 'What user would you like to see the profile of?'
                }
            ]
        });
    }

    async run({ message, client }: CommandMessageContext, { user }: { user: User }): Promise<void> {
        const { master: { database } } = client;

        const genderAttribute = await database.textAttributes.get('gender', user);
        const birthdayAttribute = await database.birthdays.get(user);
        const ageAttribute = await database.integerAttributes.get('age', user);
        const locationAttribute = await database.locationAttributes.get(user);

        const parsedBirthday = birthdayAttribute ? moment.utc(birthdayAttribute.birthday, 'YYYY-MM-DD') : null;

        await client.sendEmbed(message.channel,
            DiscordEmbedFormatter
                .baseUserEmbed(user)
                .addField('Age',
                    parsedBirthday !== null && parsedBirthday.year() !== 1804 ?
                        moment.utc().diff(parsedBirthday, 'years') :
                        ageAttribute ?
                            `${ageAttribute.integer_value} ⚠` :
                            'Not Set 🚫',
                    true
                )
                .addField('Gender', genderAttribute ? genderAttribute.attribute_value : 'Not Set 🚫', true)
                .addField('Location',
                    locationAttribute !== null ?
                        ProfileCommand.formatLocation(locationAttribute) :
                        'Not Set 🚫',
                    true
                )
        );
    }

    static formatLocation(location: { user_id: string; formatted_address: string; longitude: string; latitude: string; timezone_id: string }): string {
        return `${location.formatted_address} (${moment.utc().tz(location.timezone_id).format('LT')})`;
    }
}

export = ProfileCommand;
