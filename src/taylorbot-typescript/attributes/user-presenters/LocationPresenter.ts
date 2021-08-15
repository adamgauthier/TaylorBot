import moment = require('moment-timezone');

import { DiscordEmbedFormatter } from '../../modules/discord/DiscordEmbedFormatter';
import { UserAttributePresenter } from '../UserAttributePresenter.js';
import { User, MessageEmbed } from 'discord.js';
import { CommandMessageContext } from '../../commands/CommandMessageContext';

export class LocationPresenter implements UserAttributePresenter {
    present(commandContext: CommandMessageContext, user: User, location: Record<string, any> & { rank: string }): Promise<MessageEmbed> {
        return Promise.resolve(
            DiscordEmbedFormatter
                .baseUserSuccessEmbed(user)
                .setDescription([
                    `${user.username}'s location is **${location.formatted_address}**.`,
                    `It is currently **${moment.utc().tz(location.timezone_id).format('LT')}** there.`
                ].join('\n'))
        );
    }
}
