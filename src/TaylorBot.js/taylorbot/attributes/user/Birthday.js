'use strict';

const moment = require('moment');

const SettableUserAttribute = require('../SettableUserAttribute.js');
const SimplePresentor = require('../user-presentors/SimplePresentor.js');

class BirthdayUserAttribute extends SettableUserAttribute {
    constructor() {
        super({
            id: 'birthday',
            aliases: ['bd'],
            description: 'birthday',
            value: {
                label: 'date',
                type: 'birthday',
                example: '1989-12-13'
            },
            presentor: SimplePresentor,
            canList: true
        });
    }

    retrieve(database, user) {
        return database.birthdays.get(user);
    }

    async set(database, user, value) {
        if (value.year() === 1804) {
            const age = await database.integerAttributes.get('age', user);
            if (age) {
                await database.integerAttributes.clear('age', user);
                const now = moment.utc();
                if (now.isBefore(value.year(now.year()))) {
                    value.year(now.year() - (age.integer_value + 1));
                }
                else {
                    value.year(now.year() - age.integer_value);
                }
            }
        }

        return database.birthdays.set(user, value.format('YYYY-MM-DD'));
    }

    list(database, guild, entries) {
        return database.birthdays.getUpcomingInGuild(guild, entries);
    }

    clear(database, user) {
        return database.birthdays.clear(user);
    }

    formatValue(attribute) {
        const birthdayString = attribute.next_birthday || attribute.birthday;

        const parsed = moment.utc(birthdayString, 'YYYY-MM-DD');

        return parsed.format('MMMM Do');
    }
}

module.exports = BirthdayUserAttribute;