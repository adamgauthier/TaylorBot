'use strict';

const fetch = require('node-fetch');
const querystring = require('querystring');

const { API_KEY } = require('../../config/darksky.json');

class DarkSkyModule {
    static getCurrentForecast(lat, long) {
        return fetch(`https://api.darksky.net/forecast/${API_KEY}/${lat},${long}?${querystring.stringify({
            exclude: 'minutely,hourly,daily,alerts,flags',
            units: 'si'
        })}`).then(res => res.json());
    }
}

module.exports = DarkSkyModule;