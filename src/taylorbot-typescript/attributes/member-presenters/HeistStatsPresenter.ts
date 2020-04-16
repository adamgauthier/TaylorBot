import DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
import StringUtil = require('../../modules/StringUtil.js');
import MathUtil = require('../../modules/MathUtil.js');
import { SimpleStatPresenter } from './SimpleStatPresenter.js';
import { GuildMember, MessageEmbed } from 'discord.js';
import { CommandMessageContext } from '../../commands/CommandMessageContext';

export class HeistStatsPresenter extends SimpleStatPresenter {
    present(commandContext: CommandMessageContext, member: GuildMember, { heist_win_count, rank, heist_lose_count, heist_win_amount, heist_lose_amount }: Record<string, any> & { rank: string }): MessageEmbed {
        const winCount = BigInt(heist_win_count);
        const loseCount = BigInt(heist_lose_count);
        const winAmount = BigInt(heist_win_amount);
        const loseAmount = BigInt(heist_lose_amount);

        const successRate = (): string => {
            if (winCount === BigInt(0))
                return '0.00';

            const rate = ((winCount * BigInt(10000)) / (winCount + loseCount)).toString();
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