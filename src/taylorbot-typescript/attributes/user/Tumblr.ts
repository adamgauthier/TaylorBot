import DiscordFormatter = require('../../modules/DiscordFormatter.js');
import { TextUserAttribute } from '../TextUserAttribute.js';
import { SimplePresenter } from '../user-presenters/SimplePresenter.js';

class TumblrAttribute extends TextUserAttribute {
    constructor() {
        super({
            id: 'tumblr',
            aliases: [],
            description: 'Tumblr username',
            value: {
                label: 'username',
                type: 'tumblr-username',
                example: 'taylorswift'
            },
            presenter: SimplePresenter,
            canList: true
        });
    }

    formatValue(attribute: Record<string, any>): string {
        const value = attribute.attribute_value;
        return `[${DiscordFormatter.escapeDiscordMarkdown(value)}](https://${value}.tumblr.com/)`;
    }
}

export = TumblrAttribute;
