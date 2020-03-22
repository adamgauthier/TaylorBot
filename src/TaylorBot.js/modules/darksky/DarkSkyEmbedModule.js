'use strict';

const DiscordEmbedFormatter = require('../DiscordEmbedFormatter.js');

class DarkSkyEmbedModule {
    static dataPointToEmbed(dataPoint, user, formattedAddress) {
        const celsius = dataPoint.temperature;
        const fahrenheit = DarkSkyEmbedModule.celsiusToFahrenheit(celsius).toFixed(2);

        return DiscordEmbedFormatter
            .baseUserEmbed(user)
            .setTitle(dataPoint.summary)
            .setDescription([
                `${celsius}°C/${fahrenheit}°F`,
                `Wind: ${dataPoint.windSpeed} m/s`,
                `Humidity: ${Math.round(dataPoint.humidity * 100)}%`
            ].join('\n'))
            .setThumbnail(`https://darksky.net/images/weather-icons/${dataPoint.icon}.png`)
            .setFooter(formattedAddress)
            .setTimestamp(dataPoint.time * 1000);
    }

    static celsiusToFahrenheit(celsius) {
        return (celsius * 9 / 5) + 32;
    }
}

module.exports = DarkSkyEmbedModule;
