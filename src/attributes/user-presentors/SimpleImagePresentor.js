'use strict';

const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');

class SimpleImagePresentor {
    constructor(attribute) {
        this.attribute = attribute;
    }

    present(commandContext, user, attribute) {
        return DiscordEmbedFormatter
            .baseUserEmbed(user)
            .setTitle(`${user.username}'s ${this.attribute.description}`)
            .setImage(this.attribute.formatValue(attribute));
    }
}

module.exports = SimpleImagePresentor;