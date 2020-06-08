'use strict';

const Command = require('../Command.js');
const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const StringUtil = require('../../modules/StringUtil.js');
const RandomModule = require('../../modules/random/RandomModule.js');

class RollCommand extends Command {
    constructor() {
        super({
            name: 'roll',
            group: 'Points 💰',
            description: 'Rolls a number between 0 and 1989. Rolling 13, 15, 22 or 1989 will yield a reward.',
            examples: [''],
            maxDailyUseCount: 1989,

            args: []
        });
    }

    async run({ message, client }) {
        const { author, channel } = message;
        const { database } = client.master;

        const roll = await RandomModule.getRandIntInclusive(0, 1989);

        const { color, reward } = await (async () => {
            const rewards = { 13: 100, 15: 100, 22: 100, 420: 100, 1989: 5000 };
            switch (roll) {
                case 13:
                case 15:
                case 22:
                case 420: {
                    const reward = rewards[roll];
                    await database.rollStats.winRoll(author, reward);
                    return { color: '#43b581', reward };
                }
                case 1989: {
                    const reward = rewards[roll];
                    await database.rollStats.winPerfectRoll(author, reward);
                    return { color: '#00c3ff', reward };
                }
                default:
                    await database.rollStats.addRollCount(author, 1);
                    return { color: '#f04747', reward: 0 };
            }
        })();

        const numberEmoji = ['0⃣', '1⃣', '2⃣', '3⃣', '4⃣', '5⃣', '6⃣', '7⃣', '8⃣', '9⃣'];
        const paddedRoll = roll.toString().padStart(4, '0');

        return client.sendEmbed(channel, DiscordEmbedFormatter
            .baseUserHeader(author)
            .setColor(color)
            .setTitle('Rolling the Taylor Machine 🎲')
            .setDescription([
                `You get: ${[...paddedRoll].map(num => Number.parseInt(num)).map(num => numberEmoji[num]).join('')}`,
                reward === 0 ? 'Better luck next time! 😕' : `You won ${StringUtil.plural(reward, 'taypoint', '**')}! 💰`
            ].join('\n'))
        );
    }
}

module.exports = RollCommand;
