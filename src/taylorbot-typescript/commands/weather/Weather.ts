import { Command } from '../Command';
import { CommandError } from '../CommandError';
import { DarkSkyModule } from '../../modules/darksky/DarkSkyModule';
import { DarkSkyEmbedModule } from '../../modules/darksky/DarkSkyEmbedModule';
import { User } from 'discord.js';
import { CommandMessageContext } from '../CommandMessageContext';

class WeatherCommand extends Command {
    constructor() {
        super({
            name: 'weather',
            group: 'Weather 🌦',
            description: `Get current weather forecast for a user's location. Weather data is [Powered by Dark Sky](https://darksky.net/poweredby/).`,
            examples: ['@Adam#0420', 'Enchanted13'],
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

    async run({ message: { channel }, client }: CommandMessageContext, { user }: { user: User }): Promise<void> {
        const location = await client.master.database.locationAttributes.get(user);
        if (!location)
            throw new CommandError(`${user.username}'s location is not set. They can use the \`setlocation\` command to set it.`);

        const { currently } = await DarkSkyModule.getCurrentForecast(location.latitude, location.longitude);

        await client.sendEmbed(channel,
            DarkSkyEmbedModule.dataPointToEmbed(currently, user, location.formatted_address)
        );
    }
}

export = WeatherCommand;
