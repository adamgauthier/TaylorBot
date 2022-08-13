import moment = require('moment');
import { DiscordEmbedFormatter } from '../../modules/discord/DiscordEmbedFormatter';
import { StringUtil } from '../../modules/util/StringUtil';
import { SimpleStatPresenter } from './SimpleStatPresenter';
import { EmbedBuilder, GuildMember } from 'discord.js';
import { CommandMessageContext } from '../../commands/CommandMessageContext';
import { MathUtil } from '../../modules/util/MathUtil';

export class MinutesPresenter extends SimpleStatPresenter {
    present(commandContext: CommandMessageContext, member: GuildMember, { [this.attribute.columnName]: minuteCount, rank }: Record<string, any> & { rank: string }): EmbedBuilder {
        const duration = moment.duration(minuteCount, 'minutes');

        const description = [
            `${member.displayName} has been active for ${StringUtil.plural(minuteCount, 'minute', '**')} in this server.`,
            `This is roughly equivalent to **${duration.humanize()}** of activity.`,
        ];

        if (rank !== null && rank !== undefined) {
            description.push(`They are ranked **${MathUtil.formatNumberSuffix(Number.parseInt(rank))}** in this server (excluding users who left).`);
        }

        return DiscordEmbedFormatter
            .baseUserSuccessEmbed(member.user)
            .setDescription(description.join('\n'));
    }
}
