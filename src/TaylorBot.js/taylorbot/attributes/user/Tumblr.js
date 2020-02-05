'use strict';

const TextUserAttribute = require('../TextUserAttribute.js');
const DiscordFormatter = require('../../modules/DiscordFormatter.js');

class TumblrAttribute extends TextUserAttribute {
    constructor() {
        super({
            id: 'tumblr',
            description: 'Tumblr username',
            value: {
                label: 'username',
                type: 'tumblr-username',
                example: 'taylorswift'
            },
            canList: true
        });
    }

    format(attribute) {
        const value = attribute.attribute_value;
        return `[${DiscordFormatter.escapeDiscordMarkdown(value)}](https://${value}.tumblr.com/)`;
    }
}

module.exports = TumblrAttribute;
