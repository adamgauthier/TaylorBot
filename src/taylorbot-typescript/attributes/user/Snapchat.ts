import { Format } from '../../modules/discord/DiscordFormatter';
import { TextUserAttribute } from '../TextUserAttribute.js';
import { SimplePresenter } from '../user-presenters/SimplePresenter.js';

class SnapchatAttribute extends TextUserAttribute {
    constructor() {
        super({
            id: 'snapchat',
            aliases: ['snap'],
            description: 'Snapchat username',
            value: {
                label: 'username',
                type: 'snapchat-username',
                example: 'taylorswift'
            },
            presenter: SimplePresenter,
            canList: true
        });
    }

    formatValue(attribute: Record<string, any>): string {
        const value = attribute.attribute_value;
        return `[${Format.escapeDiscordMarkdown(value)}](https://www.snapchat.com/add/${value})`;
    }
}

export = SnapchatAttribute;
