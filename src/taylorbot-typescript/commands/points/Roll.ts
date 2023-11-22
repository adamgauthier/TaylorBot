import { Command } from '../Command';
import { DiscordEmbedFormatter } from '../../modules/discord/DiscordEmbedFormatter';
import { StringUtil } from '../../modules/util/StringUtil';
import { RandomModule } from '../../modules/random/RandomModule';
import { CommandMessageContext } from '../CommandMessageContext';
import { HexColorString } from 'discord.js';
import moment = require('moment');

class RollCommand extends Command {
    constructor() {
        super({
            name: 'roll',
            group: 'Points 💰',
            description: 'Rolls a number between 0 and 1989. Rolling 1, 7, 13, 15, 22 or 1989 will yield a reward.',
            examples: [''],
            maxDailyUseCount: 1989,

            args: []
        });
    }

    async run({ author, message, client }: CommandMessageContext): Promise<void> {
        const { channel } = message;
        const { database } = client.master;

        const windowStart = moment.utc('2023-11-22T00:00:00Z').subtract(6, 'hours');
        const windowEnd = moment.utc('2023-11-23T00:00:00Z').add(6, 'hours');
        const isAnniversary = message.guildId === '115332333745340416' && moment.utc().isBetween(windowStart, windowEnd);

        const roll = await RandomModule.getRandIntInclusive(0, 1989);

        const { color, reward } = await (async (): Promise<{ color: HexColorString; reward: number }> => {
            switch (roll) {
                case 1:
                case 7:
                case 13:
                case 15:
                case 22:
                case 420: {
                    const reward = isAnniversary ? 200 : 100;
                    await database.rollStats.winRoll(author, reward);
                    return { color: '#43b581', reward };
                }
                case 1989: {
                    const reward = isAnniversary ? 10_000 : 5_000;
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

        await client.sendEmbed(channel, DiscordEmbedFormatter
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

export = RollCommand;
