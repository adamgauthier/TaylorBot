import moment = require('moment-timezone');

import DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
import { UserAttribute } from '../UserAttribute.js';
import { UserAttributePresenter } from '../UserAttributePresenter.js';
import { User, MessageEmbed } from 'discord.js';
import CommandMessageContext = require('../../commands/CommandMessageContext.js');

export class LocationPresenter implements UserAttributePresenter {
    constructor(_: UserAttribute) { }

    present(commandContext: CommandMessageContext, user: User, location: Record<string, any> & { rank: string }): Promise<MessageEmbed> {
        return Promise.resolve(
            DiscordEmbedFormatter
                .baseUserEmbed(user)
                .setDescription([
                    `${user.username}'s location is **${location.formatted_address}**.`,
                    `It is currently **${moment.utc().tz(location.timezone_id).format('LT')}** there.`
                ].join('\n'))
        );
    }
}
