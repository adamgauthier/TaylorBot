'use strict';

const TextUserAttribute = require('../TextUserAttribute.js');
const DiscordFormatter = require('../../modules/DiscordFormatter.js');

class InstagramAttribute extends TextUserAttribute {
    constructor() {
        super({
            id: 'instagram',
            aliases: ['insta'],
            description: 'Instagram username',
            value: {
                label: 'username',
                type: 'instagram-username',
                example: 'taylorswift'
            },
            canList: true
        });
    }

    formatValue(attribute) {
        const value = attribute.attribute_value;
        return `[${DiscordFormatter.escapeDiscordMarkdown(value)}](https://www.instagram.com/${value}/)`;
    }
}

module.exports = InstagramAttribute;
