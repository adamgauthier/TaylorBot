'use strict';

const SettableUserAttribute = require('../SettableUserAttribute.js');
const GoogleTimezoneModule = require('../../modules/google/GoogleTimezoneModule.js');
const CommandError = require('../../commands/CommandError.js');
const LocationPresentor = require('../user-presentors/LocationPresentor.js');

class LocationAttribute extends SettableUserAttribute {
    constructor() {
        super({
            id: 'location',
            aliases: ['country', 'time'],
            description: 'location',
            value: {
                label: 'location',
                type: 'google-place',
                example: 'Nashville'
            },
            presentor: LocationPresentor
        });
    }

    retrieve(database, user) {
        return database.locationAttributes.get(user);
    }

    async set(database, user, value) {
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

    clear(database, user) {
        return database.locationAttributes.clear(user);
    }

    formatValue(attribute) {
        return attribute.formatted_address;
    }
}

module.exports = LocationAttribute;