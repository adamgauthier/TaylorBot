'use strict';

const Command = require('../Command.js');
const CommandError = require('../CommandError.js');
const DarkSkyModule = require('../../modules/darksky/DarkSkyModule.js');
const DarkSkyEmbedModule = require('../../modules/darksky/DarkSkyEmbedModule.js');

class WeatherCommand extends Command {
    constructor() {
        super({
            name: 'weather',
            group: 'weather',
            description: `Get current weather forecast for a user's location. Weather data is [Powered by Dark Sky](https://darksky.net/poweredby/).`,
            examples: ['@Enchanted13#1989', 'Enchanted13'],
            maxDailyUseCount: 15,

            args: [
                {
                    key: 'user',
                    label: 'user',
                    type: 'user-or-author',
                    prompt: 'What user would you like to see the weather for?'
                }
            ]
        });
    }

    async run({ message: { channel }, client }, { user }) {
        const location = await client.master.database.locationAttributes.get(user);
        if (!location)
            throw new CommandError(`${user.username}'s location is not set.`);

        const { currently } = await DarkSkyModule.getCurrentForecast(location.latitude, location.longitude);

        return client.sendEmbed(channel,
            DarkSkyEmbedModule.dataPointToEmbed(currently, user, location.formatted_address)
        );
    }
}

module.exports = WeatherCommand;