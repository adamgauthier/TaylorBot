import { Command } from '../Command';
import { CommandMessageContext } from '../CommandMessageContext';

class WolframCommand extends Command {
    constructor() {
        super({
            name: 'wolfram',
            aliases: ['wolframalpha', 'wa'],
            group: 'Knowledge ❓',
            description: 'Search on Wolfram|Alpha!',
            examples: ['This command has been moved to </wolframalpha:1082193237210574910>. Use it instead!'],
            maxDailyUseCount: 10,

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
        await client.sendEmbedError(message.channel, 'This command has been moved to </wolframalpha:1082193237210574910>. Please use it instead! 😊');
    }
}

export = WolframCommand;
