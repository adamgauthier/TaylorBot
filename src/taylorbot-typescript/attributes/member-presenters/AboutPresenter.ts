import { DiscordEmbedFormatter } from '../../modules/discord/DiscordEmbedFormatter';
import { StringUtil } from '../../modules/util/StringUtil';
import { Format } from '../../modules/discord/DiscordFormatter';
import { SimpleStatPresenter } from './SimpleStatPresenter';
import { MessageEmbed, GuildMember } from 'discord.js';
import { CommandMessageContext } from '../../commands/CommandMessageContext';
import { MathUtil } from '../../modules/util/MathUtil';

export class AboutPresenter extends SimpleStatPresenter {
    present(commandContext: CommandMessageContext, member: GuildMember, { [this.attribute.columnName]: stat, rank }: Record<string, any> & { rank: string }): MessageEmbed {
        return DiscordEmbedFormatter
            .baseUserSuccessEmbed(member.user)
            .setDescription([
                `${member.displayName} has ~${StringUtil.plural(stat, this.attribute.singularName, '**')}.`,
                `They are ranked **${MathUtil.formatNumberSuffix(Number.parseInt(rank))}** in this server (excluding users who left).`
            ].join('\n'));
    }

    presentRankEntry(member: GuildMember, { [this.attribute.columnName]: stat, rank }: Record<string, any> & { rank: string }): string {
        return `${rank}: ${Format.escapeDiscordMarkdown(member.user.username)} - ~${StringUtil.plural(stat, this.attribute.singularName, '`')}`;
    }
}
