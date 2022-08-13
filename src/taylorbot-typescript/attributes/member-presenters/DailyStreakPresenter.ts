import { GuildMember, EmbedBuilder } from 'discord.js';
import { StringUtil } from '../../modules/util/StringUtil';
import { Format } from '../../modules/discord/DiscordFormatter';
import { MemberAttributePresenter } from '../MemberAttributePresenter';
import { MemberAttribute } from '../MemberAttribute';
import { EmbedUtil } from '../../modules/discord/EmbedUtil';

export class DailyStreakPresenter implements MemberAttributePresenter {
    readonly #attribute: MemberAttribute;

    constructor(attribute: MemberAttribute) {
        this.#attribute = attribute;
    }

    present(): EmbedBuilder {
        return EmbedUtil.error('This command has been removed. Please use **/daily streak** instead.');
    }

    presentRankEntry(member: GuildMember, attribute: Record<string, any> & { rank: string }): string {
        const stat = attribute[this.#attribute.columnName];
        const { rank } = attribute;
        return `${rank}: ${Format.escapeDiscordMarkdown(member.user.username)} - ${StringUtil.plural(stat, 'day', '`', true)}`;
    }
}
