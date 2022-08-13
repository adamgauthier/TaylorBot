import { DiscordEmbedFormatter } from '../../modules/discord/DiscordEmbedFormatter';
import { StringUtil } from '../../modules/util/StringUtil';
import { Format } from '../../modules/discord/DiscordFormatter';
import { SimpleStatPresenter } from './SimpleStatPresenter';
import { EmbedBuilder, GuildMember } from 'discord.js';
import { CommandMessageContext } from '../../commands/CommandMessageContext';
import { MathUtil } from '../../modules/util/MathUtil';

export class AboutPresenter extends SimpleStatPresenter {
    present(commandContext: CommandMessageContext, member: GuildMember, { [this.attribute.columnName]: stat, rank }: Record<string, any> & { rank: string }): EmbedBuilder {
        const description = [
            `${member.displayName} has ~${StringUtil.plural(stat, this.attribute.singularName, '**')}.`
        ];

        if (rank !== null && rank !== undefined) {
            description.push(`They are ranked **${MathUtil.formatNumberSuffix(Number.parseInt(rank))}** in this server (excluding users who left).`);
        }

        return DiscordEmbedFormatter
            .baseUserSuccessEmbed(member.user)
            .setDescription(description.join('\n'));
    }

    presentRankEntry(member: GuildMember, { [this.attribute.columnName]: stat, rank }: Record<string, any> & { rank: string }): string {
        return `${rank}: ${Format.escapeDiscordMarkdown(member.user.username)} - ~${StringUtil.plural(stat, this.attribute.singularName, '`')}`;
    }
}
