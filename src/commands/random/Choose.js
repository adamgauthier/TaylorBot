'use strict';

const Command = require('../Command.js');
const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const ArrayUtil = require('../../modules/ArrayUtil.js');

class ChooseCommand extends Command {
    constructor() {
        super({
            name: 'choose',
            aliases: ['choice'],
            group: 'random',
            description: 'Chooses a random option for you.',
            examples: ['Cake, Pie'],

            args: [
                {
                    key: 'options',
                    label: 'option1,option2,...',
                    type: 'comma-separated-options',
                    prompt: 'What are the options (comma separated) to choose from?'
                }
            ]
        });
    }

    async run({ message, client }, { options }) {
        const { author, channel } = message;

        const option = ArrayUtil.random(options);

        return client.sendEmbed(channel, DiscordEmbedFormatter
            .baseUserEmbed(author)
            .setTitle('I choose:')
            .setDescription(option)
        );
    }
}

module.exports = ChooseCommand;