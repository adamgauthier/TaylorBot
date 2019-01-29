'use strict';

const moment = require('moment');
const fetch = require('node-fetch');
const querystring = require('querystring');

const { googleAPIKey } = require('../../config/google.json');

class GoogleTimezoneModule {
    static getCurrentTimeForLocation(latitude, longitude) {
        return fetch(`https://maps.googleapis.com/maps/api/timezone/json?${querystring.stringify({
            key: googleAPIKey,
            timestamp: moment.utc().format('X'),
            location: `${latitude},${longitude}`
        })}`).then(res => res.json());
    }
}

module.exports = GoogleTimezoneModule;