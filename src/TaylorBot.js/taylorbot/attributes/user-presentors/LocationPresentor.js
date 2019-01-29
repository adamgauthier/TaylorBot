'use strict';

const moment = require('moment-timezone');

const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');

class LocationPresentor {
    constructor(attribute) {
        this.attribute = attribute;
    }

    present(commandContext, user, location) {
        return DiscordEmbedFormatter
            .baseUserEmbed(user)
            .setDescription([
                `${user.username}'s location is **${location.formatted_address}**.`,
                `It is currently **${moment.utc().tz(location.timezone_id).format('LT')}** there.`
            ].join('\n'));
    }
}

module.exports = LocationPresentor;