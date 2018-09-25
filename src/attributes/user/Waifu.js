'use strict';

const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const TextUserAttribute = require('../TextUserAttribute.js');

class WaifuAttribute extends TextUserAttribute {
    constructor() {
        super({
            id: 'waifu',
            description: 'waifu',
            value: {
                label: 'waifu',
                type: 'http-url',
                example: 'https://www.example.com/link/to/picture.jpg'
            }
        });
    }

    async getEmbed(commandContext, user, attribute) {
        return DiscordEmbedFormatter
            .baseUserEmbed(user)
            .setTitle(`${user.username}'s ${this.description}`)
            .setImage(attribute);
    }
}

module.exports = WaifuAttribute;