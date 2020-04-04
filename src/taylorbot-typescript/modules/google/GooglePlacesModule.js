'use strict';

const fetch = require('node-fetch');
const querystring = require('querystring');

const { googleAPIKey } = require('../../config/google.json');

class GooglePlacesModule {
    static findPlaceFromText(input) {
        return fetch(`https://maps.googleapis.com/maps/api/place/findplacefromtext/json?${querystring.stringify({
            key: googleAPIKey,
            inputtype: 'textquery',
            fields: 'formatted_address,geometry/location',
            input
        })}`).then(res => res.json());
    }
}

module.exports = GooglePlacesModule;