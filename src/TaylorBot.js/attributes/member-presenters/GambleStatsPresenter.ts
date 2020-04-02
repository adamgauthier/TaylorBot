import { SimpleStatPresenter } from './SimpleStatPresenter';
import { CommandMessageContext } from '../../commands/CommandMessageContext';
import { GuildMember, MessageEmbed } from 'discord.js';

import DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
import StringUtil = require('../../modules/StringUtil.js');
import MathUtil = require('../../modules/MathUtil.js');

export class GambleStatsPresenter extends SimpleStatPresenter {
    present(commandContext: CommandMessageContext, member: GuildMember, { gamble_win_count, rank, gamble_lose_count, gamble_win_amount, gamble_lose_amount }: Record<string, any> & { rank: string }): MessageEmbed {
        const winCount = BigInt(gamble_win_count);
        const loseCount = BigInt(gamble_lose_count);
        const winAmount = BigInt(gamble_win_amount);
        const loseAmount = BigInt(gamble_lose_amount);

        const winRate = (): string => {
            if (winCount === BigInt(0))
                return '0.00';

            const rate = ((winCount * BigInt(10000)) / (winCount + loseCount)).toString();
            return `${rate.substring(0, rate.length - 2)}.${rate[rate.length - 2]}${rate[rate.length - 1]}`;
        };

        return DiscordEmbedFormatter
            .baseUserEmbed(member.user)
            .setDescription([
                `${member.displayName} has won a total of ${StringUtil.plural(winCount, 'gamble', '**')} (**${MathUtil.formatNumberSuffix(rank)}** in the server).`,
                `They have also lost ${StringUtil.plural(loseCount, 'gamble', '**')}, meaning they have a win rate of **${winRate()}%**.`,
                `Overall, they won ${StringUtil.plural(winAmount, 'taypoint', '**')} and lost ${StringUtil.plural(loseAmount, 'taypoint', '**')} through gambling.`
            ].join('\n'));
    }
}
