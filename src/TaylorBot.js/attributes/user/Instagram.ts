import DiscordFormatter = require('../../modules/DiscordFormatter.js');
import { TextUserAttribute } from '../TextUserAttribute.js';
import { SimplePresenter } from '../user-presenters/SimplePresenter.js';

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
            presenter: SimplePresenter,
            canList: true
        });
    }

    formatValue(attribute: Record<string, any>): string {
        const value = attribute.attribute_value;
        return `[${DiscordFormatter.escapeDiscordMarkdown(value)}](https://www.instagram.com/${value}/)`;
    }
}

export = InstagramAttribute;
