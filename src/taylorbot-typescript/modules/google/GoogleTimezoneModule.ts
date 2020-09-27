import moment = require('moment');
import fetch = require('node-fetch');
import querystring = require('querystring');

import { googleAPIKey } from '../../config/google.json';

type TimezoneResponse = {
    status: string;
    timeZoneId: string;
};

export class GoogleTimezoneModule {
    static async getCurrentTimeForLocation(latitude: number, longitude: number): Promise<TimezoneResponse> {
        return (await fetch(`https://maps.googleapis.com/maps/api/timezone/json?${querystring.stringify({
            key: googleAPIKey,
            timestamp: moment.utc().format('X'),
            location: `${latitude},${longitude}`
        })}`).then(res => res.json())) as TimezoneResponse;
    }
}
