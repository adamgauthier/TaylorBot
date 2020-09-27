import { Command } from '../Command';
import { CommandError } from '../CommandError';
import WolframAlpha = require('../../modules/wolfram/WolframAlphaModule.js');
import DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
import { CommandMessageContext } from '../CommandMessageContext';

class WolframCommand extends Command {
    constructor() {
        super({
            name: 'wolfram',
            aliases: ['wolframalpha', 'wa'],
            group: 'Knowledge ❓',
            description: 'Search on Wolfram|Alpha!',
            examples: ['convert 5km to miles', 'how many stars are there'],
            maxDailyUseCount: 10,

            args: [
                {
                    key: 'input',
                    label: 'input',
                    type: 'text',
                    prompt: 'What would you like to know?'
                }
            ]
        });
    }

    async run({ message, client }: CommandMessageContext, { input }: { input: string }): Promise<void> {
        const { author, channel } = message;

        const result = await WolframAlpha.query(input);
        if (!result.success || result.error)
            throw new CommandError(`Wolfram|Alpha did not understand '${input}'.`);

        if (result.numpods < 2)
            throw new CommandError(`Wolfram|Alpha produced unexpected results for input '${input}'.`);

        const inputPod = result.pods[0].subpods[0];
        const resultPod = result.pods[1].subpods[0];

        await client.sendEmbed(channel,
            DiscordEmbedFormatter
                .baseUserEmbed(author)
                .setTitle(inputPod.plaintext)
                .setImage(resultPod.img.src)
                .setFooter(`Wolfram|Alpha - Query took ${result.timing} seconds`, 'https://i.imgur.com/aHl1jlS.png')
        );
    }
}

export = WolframCommand;
