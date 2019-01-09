'use strict';

const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');

class SimplePresentor {
    constructor(attribute) {
        this.attribute = attribute;
    }

    present(commandContext, user, attribute) {
        return DiscordEmbedFormatter
            .baseUserEmbed(user)
            .setTitle(`${user.username}'s ${this.attribute.description}`)
            .setDescription(this.attribute.formatValue(attribute));
    }
}

module.exports = SimplePresentor;