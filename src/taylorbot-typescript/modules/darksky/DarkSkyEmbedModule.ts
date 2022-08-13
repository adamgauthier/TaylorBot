import { EmbedBuilder, User } from 'discord.js';
import { DiscordEmbedFormatter } from '../discord/DiscordEmbedFormatter';
import { DarkSkyCurrently } from './DarkSkyModule';

export class DarkSkyEmbedModule {
    static dataPointToEmbed(dataPoint: DarkSkyCurrently, user: User, formattedAddress: string): EmbedBuilder {
        const celsius = dataPoint.temperature;
        const fahrenheit = DarkSkyEmbedModule.celsiusToFahrenheit(celsius).toFixed(2);

        return DiscordEmbedFormatter
            .baseUserSuccessEmbed(user)
            .setTitle(dataPoint.summary)
            .setDescription([
                `${celsius}°C/${fahrenheit}°F`,
                `Wind: ${dataPoint.windSpeed} m/s`,
                `Humidity: ${Math.round(dataPoint.humidity * 100)}%`
            ].join('\n'))
            .setThumbnail(`https://darksky.net/images/weather-icons/${dataPoint.icon}.png`)
            .setFooter({ text: formattedAddress })
            .setTimestamp(dataPoint.time * 1000);
    }

    static celsiusToFahrenheit(celsius: number): number {
        return (celsius * 9 / 5) + 32;
    }
}
