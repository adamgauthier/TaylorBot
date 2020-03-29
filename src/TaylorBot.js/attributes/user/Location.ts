import GoogleTimezoneModule = require('../../modules/google/GoogleTimezoneModule.js');
import CommandError = require('../../commands/CommandError.js');
import { SettableUserAttribute } from '../SettableUserAttribute';
import { LocationPresenter } from '../user-presenters/LocationPresenter';
import { DatabaseDriver } from '../../database/DatabaseDriver';
import { User } from 'discord.js';

class LocationAttribute extends SettableUserAttribute {
    constructor() {
        super({
            id: 'location',
            aliases: ['country', 'time'],
            description: 'location',
            value: {
                label: 'location',
                type: 'google-place',
                example: 'Nashville',
                maxDailySetCount: 3
            },
            presenter: LocationPresenter,
            list: null
        });
    }

    retrieve(database: DatabaseDriver, user: User): Promise<any> {
        return database.locationAttributes.get(user);
    }

    async set(database: DatabaseDriver, user: User, value: any): Promise<Record<string, any>> {
        const { geometry: { location: { lat, lng } }, formatted_address } = value;
        const response = await GoogleTimezoneModule.getCurrentTimeForLocation(lat, lng);

        switch (response.status) {
            case 'OK':
                return database.locationAttributes.set(user, formatted_address, lng, lat, response.timeZoneId);
            case 'ZERO_RESULTS':
                throw new CommandError(`Can't find the timezone for '${formatted_address}', please be more specific.`);
            default:
                throw new CommandError(`Something went wrong when trying to get time from Google Maps.`);
        }
    }

    clear(database: DatabaseDriver, user: User): Promise<void> {
        return database.locationAttributes.clear(user);
    }

    formatValue(attribute: Record<string, any>): string {
        return attribute.formatted_address;
    }
}

export = LocationAttribute;
