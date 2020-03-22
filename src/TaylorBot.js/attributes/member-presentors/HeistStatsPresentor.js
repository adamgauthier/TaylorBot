'use strict';

const SimpleStatPresentor = require('./SimpleStatPresentor.js');
const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const StringUtil = require('../../modules/StringUtil.js');
const MathUtil = require('../../modules/MathUtil.js');

class HeistStatsPresentor extends SimpleStatPresentor {
    constructor(attribute) {
        super(attribute);
    }

    present(commandContext, member, { heist_win_count, rank, heist_lose_count, heist_win_amount, heist_lose_amount }) {
        const winCount = global.BigInt(heist_win_count);
        const loseCount = global.BigInt(heist_lose_count);
        const winAmount = global.BigInt(heist_win_amount);
        const loseAmount = global.BigInt(heist_lose_amount);

        const successRate = () => {
            if (winCount === global.BigInt(0))
                return '0.00';

            const rate = ((winCount * global.BigInt(10000)) / (winCount + loseCount)).toString();
            return `${rate.substring(0, rate.length - 2)}.${rate[rate.length - 2]}${rate[rate.length - 1]}`;
        };

        return DiscordEmbedFormatter
            .baseUserEmbed(member.user)
            .setDescription([
                `${member.displayName} has won a total of ${StringUtil.plural(winCount, 'heist', '**')} (**${MathUtil.formatNumberSuffix(rank)}** in the server).`,
                `They have also failed ${StringUtil.plural(loseCount, 'heist', '**')}, meaning they have a success rate of **${successRate()}%**.`,
                `Overall, they won ${StringUtil.plural(winAmount, 'taypoint', '**')} and lost ${StringUtil.plural(loseAmount, 'taypoint', '**')} through heists.`
            ].join('\n'));
    }
}

module.exports = HeistStatsPresentor;
