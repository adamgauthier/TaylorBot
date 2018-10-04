'use strict';

const moment = require('moment-timezone');

const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const Command = require('../Command.js');

class ProfileCommand extends Command {
    constructor() {
        super({
            name: 'profile',
            aliases: ['info', 'asl'],
            group: 'info',
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

    async run({ message, client }, { user }) {
        const { master: { database } } = client;

        const genderAttribute = await database.textAttributes.get('gender', user);
        const ageAttribute = await database.integerAttributes.get('age', user);
        const locationAttribute = await database.locationAttributes.get(user);

        return client.sendEmbed(message.channel,
            DiscordEmbedFormatter
                .baseUserEmbed(user)
                .addField('Age', ageAttribute ? ageAttribute.integer_value : 'Not Set 🚫', true)
                .addField('Gender', genderAttribute ? genderAttribute.attribute_value : 'Not Set 🚫', true)
                .addField('Location',
                    locationAttribute ?
                        ProfileCommand.formatLocation(locationAttribute) :
                        'Not Set 🚫',
                    true
                )
        );
    }

    static formatLocation(location) {
        return `${location.formatted_address} (${moment.utc().tz(location.timezone_id).format('LT')})`;
    }
}

module.exports = ProfileCommand;