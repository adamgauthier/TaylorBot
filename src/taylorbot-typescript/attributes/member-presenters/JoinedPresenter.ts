import { DiscordEmbedFormatter } from '../../modules/discord/DiscordEmbedFormatter';
import { TimeUtil } from '../../modules/util/TimeUtil';
import { Format } from '../../modules/discord/DiscordFormatter';
import { MemberAttributePresenter } from '../MemberAttributePresenter';
import { MessageEmbed, GuildMember } from 'discord.js';
import { MemberAttribute } from '../MemberAttribute';
import { CommandMessageContext } from '../../commands/CommandMessageContext';
import { MathUtil } from '../../modules/util/MathUtil';

export class JoinedPresenter implements MemberAttributePresenter {
    constructor(private readonly attribute: MemberAttribute) {
    }

    present(commandContext: CommandMessageContext, member: GuildMember, { [this.attribute.columnName]: firstJoinedAt, rank }: Record<string, any> & { rank: string }): MessageEmbed {
        return DiscordEmbedFormatter
            .baseUserEmbed(member.user)
            .setDescription(firstJoinedAt ? [
                `${member.displayName} first joined the server on **${TimeUtil.formatFull(firstJoinedAt.getTime())}**.`,
                `They were the **${MathUtil.formatNumberSuffix(Number.parseInt(rank))}** user to join.`
            ].join('\n') : `I don't know when ${member.displayName} first joined the server.`);
    }

    presentRankEntry(member: GuildMember, { [this.attribute.columnName]: firstJoinedAt, rank }: Record<string, any> & { rank: string }): string {
        return `${rank}: ${Format.escapeDiscordMarkdown(member.user.username)} - ${TimeUtil.formatMini(firstJoinedAt.getTime())}`;
    }
}
