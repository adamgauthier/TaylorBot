'use strict';

const TextUserAttribute = require('../TextUserAttribute.js');
const LastFmPresentor = require('../user-presentors/LastFmPresentor.js');
const DiscordFormatter = require('../../modules/DiscordFormatter.js');

class LastFmAttribute extends TextUserAttribute {
    constructor() {
        super({
            id: 'lastfm',
            aliases: ['fm', 'np'],
            description: 'Last.fm username',
            value: {
                label: 'username',
                type: 'last-fm-username',
                example: 'taylorswift'
            },
            presentor: LastFmPresentor,
            canList: true
        });
    }

    formatValue(attribute) {
        const value = attribute.attribute_value;
        return `[${DiscordFormatter.escapeDiscordMarkdown(value)}](https://www.last.fm/user/${value}/)`;
    }
}

module.exports = LastFmAttribute;
