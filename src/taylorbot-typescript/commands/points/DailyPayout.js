'use strict';

const moment = require('moment');

const Command = require('../Command.js');
const CommandError = require('../../commands/CommandError.js');
const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const StringUtil = require('../../modules/StringUtil.js');
const TimeUtil = require('../../modules/TimeUtil.js');

const PAYOUT_POINTS = 50;
const STREAK_FOR_BONUS = 5;
const BASE_BONUS_POINTS = 41;
const INCREASING_BONUS_MULTIPLIER = 4;

class DailyPayoutCommand extends Command {
    constructor() {
        super({
            name: 'dailypayout',
            aliases: ['daily'],
            group: 'Points 💰',
            description: 'Awards you with your daily amount of taypoints.',
            examples: [''],

            args: []
        });
    }

    async run({ message, client }) {
        const { author, channel } = message;
        const { database } = client.master;

        const result = await database.dailyPayouts.getCanRedeem(author);

        if (result && !result.can_redeem) {
            const reset = moment(result.can_redeem_at);
            throw new CommandError([
                'You already redeemed your daily payout today.',
                `You can redeem again on \`${TimeUtil.formatLog(reset.valueOf())}\` (${reset.fromNow()}).`
            ].join('\n'));
        }

        const payResult = await database.dailyPayouts.giveDailyPay(author, PAYOUT_POINTS, STREAK_FOR_BONUS, BASE_BONUS_POINTS, INCREASING_BONUS_MULTIPLIER);

        if (payResult === null) {
            throw new CommandError('You already redeemed your daily payout today.');
        }

        const {
            taypoint_count,
            streak_count,
            payoutCount,
            bonus_reward
        } = payResult;

        const nextStreak = (global.BigInt(streak_count) - global.BigInt(streak_count) % global.BigInt(STREAK_FOR_BONUS)) + global.BigInt(STREAK_FOR_BONUS);

        return client.sendEmbed(channel, DiscordEmbedFormatter
            .baseUserEmbed(author)
            .setDescription([
                `You redeemed ${StringUtil.plural(payoutCount, 'taypoint', '**')} + ${StringUtil.plural(
                    bonus_reward, 'bonus taypoint', '**'
                )}. You now have **${StringUtil.formatNumberString(taypoint_count)}**. 💰`,
                `Bonus streak: **${StringUtil.formatNumberString(streak_count)}**/**${StringUtil.formatNumberString(
                    nextStreak
                )}**. Don't miss a day and get a bonus! See you tomorrow! 😄`
            ].join('\n'))
        );
    }
}

module.exports = DailyPayoutCommand;
