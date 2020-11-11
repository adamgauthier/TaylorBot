import moment = require('moment');
import fetch = require('node-fetch');
import querystring = require('querystring');

import { EnvUtil } from '../util/EnvUtil';

const googleAPIKey = EnvUtil.getRequiredEnvVariable('TaylorBot_Google__ApiKey');

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
