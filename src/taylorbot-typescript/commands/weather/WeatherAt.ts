import { Command } from '../Command';
import { CommandMessageContext } from '../CommandMessageContext';

class WeatherCommand extends Command {
    constructor() {
        super({
            name: 'weatherat',
            group: 'Weather 🌦',
            description: 'This command has been removed. Please use </weather:1015857497896136755> with the **location** option instead.',
            examples: [''],

            args: [
                {
                    key: 'args',
                    label: 'args',
                    type: 'any-text',
                    prompt: 'What arguments would you like to use?'
                }
            ]
        });
    }

    async run({ message, client }: CommandMessageContext): Promise<void> {
        await client.sendEmbedError(message.channel, [
            'This command has been removed.',
            'Please use </weather:1015857497896136755> with the **location** option instead.'
        ].join('\n'));
    }
}

export = WeatherCommand;
