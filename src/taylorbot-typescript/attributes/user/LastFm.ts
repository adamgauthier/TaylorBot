import DiscordFormatter = require('../../modules/DiscordFormatter.js');
import { TextUserAttribute } from '../TextUserAttribute';
import { LastFmPresenter } from '../user-presenters/LastFmPresenter.js';

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
            presenter: LastFmPresenter,
            canList: true
        });
    }

    formatValue(attribute: Record<string, any>): string {
        const value = attribute.attribute_value;
        return `[${DiscordFormatter.escapeDiscordMarkdown(value)}](https://www.last.fm/user/${value}/)`;
    }
}

export = LastFmAttribute;
