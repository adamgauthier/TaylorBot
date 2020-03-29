import moment = require('moment');

import DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
import StringUtil = require('../../modules/StringUtil.js');
import MathUtil = require('../../modules/MathUtil.js');
import { SimpleStatPresenter } from './SimpleStatPresenter.js';
import { MessageEmbed, GuildMember } from 'discord.js';
import CommandMessageContext = require('../../commands/CommandMessageContext.js');

export class MinutesPresenter extends SimpleStatPresenter {
    present(commandContext: CommandMessageContext, member: GuildMember, { [this.attribute.columnName]: minuteCount, rank }: Record<string, any> & { rank: string }): MessageEmbed {
        const duration = moment.duration(minuteCount, 'minutes');

        return DiscordEmbedFormatter
            .baseUserEmbed(member.user)
            .setDescription([
                `${member.displayName} has been active for ${StringUtil.plural(minuteCount, 'minute', '**')} in this server.`,
                `This is roughly equivalent to **${duration.humanize()}** of activity.`,
                `They are the **${MathUtil.formatNumberSuffix(rank)}** user of the server (excluding users who left).`
            ].join('\n'));
    }
}
