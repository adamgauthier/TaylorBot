'use strict';

const Command = require('../Command.js');
const CommandError = require('../../commands/CommandError.js');
const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const StringUtil = require('../../modules/StringUtil.js');

const PAYOUT = 50;

class DailyPayoutCommand extends Command {
    constructor() {
        super({
            name: 'dailypayout',
            aliases: ['daily'],
            group: 'points',
            description: 'Awards you with your daily amount of taypoints.',
            examples: [''],

            args: []
        });
    }

    async run({ message, client }) {
        const { author, channel } = message;
        const { database } = client.master;

        const result = await database.dailyPayouts.isLastPayoutInLast24Hours(author);

        if (result && result.is_in_last_day) {
            throw new CommandError(`You already redeemed your daily payout in the last 24 hours.`);
        }

        const { taypoint_count } = await database.dailyPayouts.giveDailyPay(author, PAYOUT);

        return client.sendEmbed(channel, DiscordEmbedFormatter
            .baseUserEmbed(author)
            .setDescription([
                `You redeemed ${StringUtil.plural(PAYOUT, 'taypoint', '**')}, you now have ${taypoint_count}. 💰`,
                'See you tomorrow! 😄'
            ].join('\n'))
        );
    }
}

module.exports = DailyPayoutCommand;