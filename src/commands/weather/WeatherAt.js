'use strict';

const Command = require('../Command.js');
const DarkSkyModule = require('../../modules/darksky/DarkSkyModule.js');
const DarkSkyEmbedModule = require('../../modules/darksky/DarkSkyEmbedModule.js');

class WeatherCommand extends Command {
    constructor() {
        super({
            name: 'weatherat',
            group: 'weather',
            description: `Get current weather forecast for a location. Weather data is [Powered by Dark Sky](https://darksky.net/poweredby/).`,
            examples: ['Nashville, USA'],

            args: [
                {
                    key: 'place',
                    label: 'location',
                    type: 'google-place',
                    prompt: 'What location would you like to see the weather for?'
                }
            ]
        });
    }

    async run({ message: { channel, author }, client }, { place }) {
        const { geometry: { location: { lat, lng } }, formatted_address } = place;
        const { currently } = await DarkSkyModule.getCurrentForecast(lat, lng);

        return client.sendEmbed(channel,
            DarkSkyEmbedModule.dataPointToEmbed(currently, author, formatted_address)
        );
    }
}

module.exports = WeatherCommand;