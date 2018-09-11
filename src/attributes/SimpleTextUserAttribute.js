'use strict';

const TextUserAttribute = require('./TextUserAttribute.js');
const DiscordEmbedFormatter = require('../modules/DiscordEmbedFormatter.js');

class SimpleTextUserAttribute extends TextUserAttribute {
    constructor(options) {
        super(options);
        if (new.target === SimpleTextUserAttribute) {
            throw new Error(`Can't instantiate abstract ${this.constructor.name} class.`);
        }
    }

    getEmbed(commandContext, user, attribute) {
        return DiscordEmbedFormatter
            .baseUserEmbed(user)
            .setTitle(`${user.username}'s ${this.description}`)
            .setDescription(this.format(attribute));
    }
}

module.exports = SimpleTextUserAttribute;