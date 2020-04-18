import DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
import StringUtil = require('../../modules/StringUtil.js');
import { SimpleStatPresenter } from './SimpleStatPresenter.js';
import { MessageEmbed, GuildMember } from 'discord.js';
import { CommandMessageContext } from '../../commands/CommandMessageContext';
import { MathUtil } from '../../modules/util/MathUtil';

export class RollStatsPresenter extends SimpleStatPresenter {
    present(commandContext: CommandMessageContext, member: GuildMember, { roll_count, rank, perfect_roll_count }: Record<string, any> & { rank: string }): MessageEmbed {
        const rollCount = BigInt(roll_count);
        const perfectRollCount = BigInt(perfect_roll_count);

        const rateDecimal = (): string => {
            const rate = ((rollCount * BigInt(100)) / perfectRollCount).toString();
            return `${rate.substring(0, rate.length - 2)}.${rate[rate.length - 2]}${rate[rate.length - 1]}`;
        };

        const oddsPercent = (): string => {
            const num = BigInt(1999) ** rollCount;
            const denom = BigInt(2000) ** rollCount;

            const rate = ((num * BigInt(10000)) / denom).toString();
            return `${rate.substring(0, rate.length - 2)}.${rate[rate.length - 2]}${rate[rate.length - 1]}`;
        };

        return DiscordEmbedFormatter
            .baseUserEmbed(member.user)
            .setDescription([
                `${member.displayName} has rolled a total of ${StringUtil.plural(rollCount, 'time', '**')} (**${MathUtil.formatNumberSuffix(Number.parseInt(rank))}** in the server).`,
                `They have ${StringUtil.plural(perfect_roll_count, 'perfect roll', '**')} of 1989.`,
                perfectRollCount > 0 ?
                    `This means they roughly get a perfect roll every **${rateDecimal()}** rolls.` :
                    `The odds of not getting a perfect roll in that many rolls is roughly **${oddsPercent()}%**.`
            ].join('\n'));
    }
}
