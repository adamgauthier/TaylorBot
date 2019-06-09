'use strict';

const SimpleStatPresentor = require('./SimpleStatPresentor.js');
const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const StringUtil = require('../../modules/StringUtil.js');
const MathUtil = require('../../modules/MathUtil.js');

class RollStatsPresentor extends SimpleStatPresentor {
    constructor(attribute) {
        super(attribute);
    }

    present(commandContext, member, { roll_count, rank, perfect_roll_count }) {
        const rollCount = global.BigInt(roll_count);
        const perfectRollCount = global.BigInt(perfect_roll_count);

        const rateDecimal = () => {
            const rate = ((rollCount * global.BigInt(100)) / perfectRollCount).toString();
            return `${rate.substring(0, rate.length - 2)}.${rate[rate.length - 2]}${rate[rate.length - 1]}`;
        };

        const oddsPercent = () => {
            const num = global.BigInt(1999) ** rollCount;
            const denom = global.BigInt(2000) ** rollCount;

            const rate = ((num * global.BigInt(10000)) / denom).toString();
            return `${rate.substring(0, rate.length - 2)}.${rate[rate.length - 2]}${rate[rate.length - 1]}`;
        };

        return DiscordEmbedFormatter
            .baseUserEmbed(member.user)
            .setDescription([
                `${member.displayName} has rolled a total of ${StringUtil.plural(rollCount, 'time', '**')} (**${MathUtil.formatNumberSuffix(rank)}** in the server).`,
                `They have ${StringUtil.plural(perfect_roll_count, 'perfect roll', '**')} of 1989.`,
                perfectRollCount > 0 ?
                    `This means they roughly get a perfect roll every **${rateDecimal()}** rolls.` :
                    `The odds of not getting a perfect roll in that many rolls is roughly **${oddsPercent()}%**.`
            ].join('\n'));
    }
}

module.exports = RollStatsPresentor;
