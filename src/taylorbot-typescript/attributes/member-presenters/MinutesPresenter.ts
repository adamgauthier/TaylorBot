import moment = require('moment');
import { DiscordEmbedFormatter } from '../../modules/discord/DiscordEmbedFormatter';
import { StringUtil } from '../../modules/util/StringUtil';
import { SimpleStatPresenter } from './SimpleStatPresenter';
import { MessageEmbed, GuildMember } from 'discord.js';
import { CommandMessageContext } from '../../commands/CommandMessageContext';
import { MathUtil } from '../../modules/util/MathUtil';

export class MinutesPresenter extends SimpleStatPresenter {
    present(commandContext: CommandMessageContext, member: GuildMember, { [this.attribute.columnName]: minuteCount, rank }: Record<string, any> & { rank: string }): MessageEmbed {
        const duration = moment.duration(minuteCount, 'minutes');

        return DiscordEmbedFormatter
            .baseUserSuccessEmbed(member.user)
            .setDescription([
                `${member.displayName} has been active for ${StringUtil.plural(minuteCount, 'minute', '**')} in this server.`,
                `This is roughly equivalent to **${duration.humanize()}** of activity.`,
                `They are ranked **${MathUtil.formatNumberSuffix(Number.parseInt(rank))}** in this server (excluding users who left).`
            ].join('\n'));
    }
}
