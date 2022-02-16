import { Command } from '../Command';
import { CommandError } from '../CommandError';
import { WolframAlphaModule } from '../../modules/wolfram/WolframAlphaModule';
import { DiscordEmbedFormatter } from '../../modules/discord/DiscordEmbedFormatter';
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

    async run({ message, client, author }: CommandMessageContext, { input }: { input: string }): Promise<void> {
        const { channel } = message;

        const result = await WolframAlphaModule.query(input);
        if (!result.success || result.error)
            throw new CommandError(`Wolfram|Alpha did not understand '${input}'.`);

        if (result.numpods < 2)
            throw new CommandError(`Wolfram|Alpha produced unexpected results for input '${input}'.`);

        const inputPod = result.pods[0].subpods[0];
        const resultPod = result.pods[1].subpods[0];

        await client.sendEmbed(channel,
            DiscordEmbedFormatter
                .baseUserSuccessEmbed(author)
                .setTitle(inputPod.plaintext)
                .setImage(resultPod.img.src)
                .setFooter({ text: `Wolfram|Alpha - Query took ${result.timing} seconds`, iconURL: 'https://i.imgur.com/aHl1jlS.png' })
        );
    }
}

export = WolframCommand;
