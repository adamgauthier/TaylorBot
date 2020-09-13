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
                type: 'birthday',
                example: '1989-12-13'
            },
            presenter: SimplePresenter,
            list: (database, guild, entries) =>
                database.birthdays.getUpcomingInGuild(guild, entries)
        });
    }

    retrieve(database: DatabaseDriver, user: User): Promise<any> {
        return database.birthdays.get(user);
    }

    set(database: DatabaseDriver, user: User, value: any): Promise<Record<string, any>> {
        return this.setBirthday(database, user, value, false);
    }

    async setBirthday(database: DatabaseDriver, user: User, value: any, isPrivate: boolean): Promise<Record<string, any>> {
        const age = await database.integerAttributes.get('age', user);
        if (age) {
            await database.integerAttributes.clear('age', user);
            if (value.year() === 1804) {
                const now = moment.utc();
                if (now.isBefore(value.year(now.year()))) {
                    value.year(now.year() - (age.integer_value + 1));
                }
                else {
                    value.year(now.year() - age.integer_value);
                }
            }
        }

        return database.birthdays.set(user, value.format('YYYY-MM-DD'), isPrivate);
    }

    async clear(database: DatabaseDriver, user: User): Promise<void> {
        await database.birthdays.clear(user);
    }

    formatValue(attribute: Record<string, any>): string {
        const birthdayString = attribute.next_birthday || attribute.birthday;

        const parsed = moment.utc(birthdayString, 'YYYY-MM-DD');

        return attribute.is_private ? `This user's birthday is private` : parsed.format('MMMM Do');
    }
}

export = BirthdayUserAttribute;
