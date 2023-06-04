import { User } from 'discord.js';
import { DatabaseDriver } from '../../database/DatabaseDriver';
import { TextUserAttribute } from '../TextUserAttribute.js';
import { DeprecatedUserPresenter } from '../user-presenters/DeprecatedUserPresenter';

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
            presenter: DeprecatedUserPresenter,
            canList: false
        });
    }

    retrieve(database: DatabaseDriver, user: User): Promise<any> {
        return Promise.resolve({ newCommand: `Discord's profile connections feature` });
    }

    set(database: DatabaseDriver, user: User, value: any): Promise<string> {
        return Promise.resolve(`Discord's profile connections feature`);
    }

    clear(database: DatabaseDriver, user: User): Promise<string> {
        return Promise.resolve(`Discord's profile connections feature`);
    }
}

export = InstagramAttribute;
