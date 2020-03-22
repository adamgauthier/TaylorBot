'use strict';

const Command = require('../Command.js');
const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const StringUtil = require('../../modules/StringUtil.js');
const RandomModule = require('../../modules/random/RandomModule.js');

class GambleCommand extends Command {
    constructor() {
        super({
            name: 'gamble',
            group: 'points',
            description: 'Gamble a specified amount of taypoints. If you roll 51-100, you win the gambled amount, if you roll 1-50, you lose that amount.',
            examples: ['13', 'all'],

            args: [
                {
                    key: 'amount',
                    label: 'taypoints',
                    type: 'taypoint-amount',
                    prompt: 'How much taypoints do you want to gamble?'
                }
            ]
        });
    }

    async run({ message, client }, { amount }) {
        const { author, channel } = message;

        const limit = 100;

        const roll = await RandomModule.getRandIntInclusive(1, limit);
        const won = roll >= 51;

        const { gambled_count, original_count, final_count } = await (won ?
            client.master.database.gambleStats.winGambledTaypointCount(author, amount, '1') :
            client.master.database.gambleStats.loseGambledTaypointCount(author, amount)
        );

        return client.sendEmbed(channel, DiscordEmbedFormatter
            .baseUserHeader(author)
            .setColor(won ? '#43b581' : '#f04747')
            .setDescription([
                `Gambled ${StringUtil.plural(gambled_count, 'taypoint', '**')} out of ${StringUtil.formatNumberString(original_count)}. 💵`,
                `Rolled **${roll}**/${limit}. 🎲 You ${won ? 'won! 😄' : 'lost! 😔'}`,
                `You now have ${StringUtil.plural(final_count, 'taypoint', '**')}. ${won ? '💰' : '💸'}`
            ].join('\n'))
        );
    }
}

module.exports = GambleCommand;
