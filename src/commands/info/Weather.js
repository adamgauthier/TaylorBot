'use strict';

const Command = require('../Command.js');
const CommandError = require('../CommandError.js');
const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const DarkSkyModule = require('../../modules/darksky/DarkSkyModule.js');

class WeatherCommand extends Command {
    constructor() {
        super({
            name: 'weather',
            group: 'info',
            description: `Get current weather forecast for a user's location. Weather data is [Powered by Dark Sky](https://darksky.net/poweredby/).`,
            examples: ['@Enchanted13#1989', 'Enchanted13'],

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

        const celsius = currently.temperature;
        const fahrenheit = WeatherCommand.celsiusToFahrenheit(celsius).toFixed(2);

        return client.sendEmbed(channel, DiscordEmbedFormatter
            .baseUserEmbed(user)
            .setTitle(currently.summary)
            .setDescription([
                `${celsius}°C/${fahrenheit}°F`,
                `Wind: ${currently.windSpeed} km/h`,
                `Humidity: ${Math.round(currently.humidity * 100)}%`
            ].join('\n'))
            .setThumbnail(`https://darksky.net/images/weather-icons/${currently.icon}.png`)
            .setFooter(location.formatted_address)
            .setTimestamp(new Date(currently.time * 1000))
        );
    }

    static celsiusToFahrenheit(celsius) {
        return (celsius * 9 / 5) + 32;
    }
}

module.exports = WeatherCommand;