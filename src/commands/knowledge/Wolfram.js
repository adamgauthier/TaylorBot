'use strict';

const Command = require('../../structures/Command.js');
const CommandError = require('../../structures/CommandError.js');
const WolframAlpha = require('../../modules/wolfram/WolframAlphaModule.js');
const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');

class WolframCommand extends Command {
    constructor() {
        super({
            name: 'wolfram',
            aliases: ['wolframalpha', 'wa'],
            group: 'knowledge',
            description: 'Search on Wolfram|Alpha!',
            examples: ['convert 5km to miles', 'how many stars are there'],

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

    async run({ message, client }, { input }) {
        const { author, channel } = message;

        const result = await WolframAlpha.query(input);
        if (!result.success || result.error)
            throw new CommandError(`Wolfram|Alpha did not understand '${input}'.`);

        const inputPod = result.pods[0].subpods[0];
        const resultPod = result.pods[1].subpods[0];

        return client.sendEmbed(channel,
            DiscordEmbedFormatter
                .baseUserEmbed(author)
                .setTitle(inputPod.plaintext)
                .setImage(resultPod.img.src)
                .setFooter(`Wolfram|Alpha - Query took ${result.timing} seconds`, 'https://i.imgur.com/aHl1jlS.png')
        );
    }
}

module.exports = WolframCommand;