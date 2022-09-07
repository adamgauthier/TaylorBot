import moment = require('moment');

import { SettableUserAttribute } from '../SettableUserAttribute.js';
import { SimplePresenter } from '../user-presenters/SimplePresenter.js';
import { DatabaseDriver } from '../../database/DatabaseDriver.js';
import { User } from 'discord.js';

class BirthdayUserAttribute extends SettableUserAttribute {
    constructor() {
        super({
            id: 'birthday',
            aliases: ['bd', 'bday', 'birthdays'],
            description: 'birthday',
            value: {
                label: 'date',
                type: 'any-text',
                example: '1989-12-13'
            },
            presenter: SimplePresenter,
            list: '</birthday calendar:1016938623880400907>'
        });
    }

    retrieve(database: DatabaseDriver, user: User): Promise<any> {
        return database.birthdays.get(user);
    }

    async set(database: DatabaseDriver, user: User, value: any): Promise<string | Record<string, any>> {
        return '</birthday set:1016938623880400907>';
    }

    async clear(database: DatabaseDriver, user: User): Promise<string | void> {
        return '</birthday clear:1016938623880400907>';
    }

    formatValue(attribute: Record<string, any>): string {
        const parsed = moment.utc(attribute.birthday, 'YYYY-MM-DD');

        const firstLine = attribute.is_private ? `This user's birthday is private` : parsed.format('MMMM Do');

        return firstLine + '\nPlease use </birthday show:1016938623880400907> instead! 😊';
    }
}

export = BirthdayUserAttribute;
