import { CommandMessageContext } from '../../commands/CommandMessageContext';
import { GuildMember, MessageEmbed } from 'discord.js';
import DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
import StringUtil = require('../../modules/StringUtil.js');
import DiscordFormatter = require('../../modules/DiscordFormatter.js');
import { MemberAttributePresenter } from '../MemberAttributePresenter';
import { MemberAttribute } from '../MemberAttribute';
import { MathUtil } from '../../modules/util/MathUtil';

export class DailyStreakPresenter implements MemberAttributePresenter {
    readonly #attribute: MemberAttribute;

    constructor(attribute: MemberAttribute) {
        this.#attribute = attribute;
    }

    present(commandContext: CommandMessageContext, member: GuildMember, attribute: Record<string, any> & { rank: string }): MessageEmbed {
        const streak = attribute[this.#attribute.columnName];
        const { rank } = attribute;
        return DiscordEmbedFormatter
            .baseUserEmbed(member.user)
            .setDescription([
                `${member.displayName} is currently on a **${StringUtil.formatNumberString(streak)}** day payout streak.`,
                `They are the **${MathUtil.formatNumberSuffix(Number.parseInt(rank))}** user of the server (excluding users who left).`
            ].join('\n'));
    }

    presentRankEntry(member: GuildMember, attribute: Record<string, any> & { rank: string }): string {
        const stat = attribute[this.#attribute.columnName];
        const { rank } = attribute;
        return `${rank}: ${DiscordFormatter.escapeDiscordMarkdown(member.user.username)} - ${StringUtil.plural(stat, 'day', '`', true)}`;
    }
}
