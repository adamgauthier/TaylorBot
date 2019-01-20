'use strict';

const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const StringUtil = require('../../modules/StringUtil.js');
const MathUtil = require('../../modules/MathUtil.js');

class GambleStatsPresentor {
    constructor(attribute) {
        this.attribute = attribute;
    }

    present(commandContext, member, { gamble_win_count, rank, gamble_lose_count, gamble_win_amount, gamble_lose_amount }) {
        const winCount = global.BigInt(gamble_win_count);
        const loseCount = global.BigInt(gamble_lose_count);
        const winAmount = global.BigInt(gamble_win_amount);
        const loseAmount = global.BigInt(gamble_lose_amount);

        const winRate = ((winCount * global.BigInt(10000)) / (winCount + loseCount)).toString();

        const winRatePercent = `${winRate.substring(0, winRate.length - 2)}.${winRate[winRate.length - 2]}${winRate[winRate.length - 1]}`;

        return DiscordEmbedFormatter
            .baseUserEmbed(member.user)
            .setDescription([
                `${member.displayName} has won a total of ${StringUtil.plural(winCount, 'gamble', '**')} (**${MathUtil.formatNumberSuffix(rank)}** in the server).`,
                `They have also lost ${StringUtil.plural(loseCount, 'gamble', '**')}, meaning they have a win rate of **${winRatePercent}%**.`,
                `Overall, they won ${StringUtil.plural(winAmount, 'taypoint', '**')} and lost ${StringUtil.plural(loseAmount, 'taypoint', '**')} through gambling.`
            ].join('\n'));
    }

    presentRankEntry(member, { [this.attribute.columnName]: stat, rank }) {
        return `${rank}: ${member.user.username} - ${StringUtil.plural(stat, this.attribute.singularName, '`')}`;
    }
}

module.exports = GambleStatsPresentor;