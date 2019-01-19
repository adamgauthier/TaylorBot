'use strict';

const Command = require('../Command.js');
const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const MathUtil = require('../../modules/MathUtil.js');
const StringUtil = require('../../modules/StringUtil.js');

class DiceCommand extends Command {
    constructor() {
        super({
            name: 'dice',
            group: 'random',
            description: 'Rolls a dice with the specified amount of faces.',
            examples: ['6', '13', '22'],

            args: [
                {
                    key: 'faces',
                    label: 'faces',
                    type: 'strictly-positive-integer',
                    prompt: 'How many faces should your dice have?'
                }
            ]
        });
    }

    async run({ message, client }, { faces }) {
        const { author, channel } = message;

        const roll = MathUtil.getRandomInt(1, faces);

        return client.sendEmbed(channel, DiscordEmbedFormatter
            .baseUserEmbed(author)
            .setTitle(`Rolling a dice with ${StringUtil.plural(faces, 'face')} 🎲`)
            .setDescription(`You rolled **${roll}**!`)
        );
    }
}

module.exports = DiceCommand;