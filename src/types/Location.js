'use strict';

const TextArgumentType = require('./Text.js');
const ArgumentParsingError = require('../structures/ArgumentParsingError.js');
const GooglePlacesModule = require('../modules/google/GooglePlacesModule.js');

class GooglePlaceArgumentType extends TextArgumentType {
    get id() {
        return 'google-place';
    }

    async parse(val) {
        const response = await GooglePlacesModule.findPlaceFromText(val);

        switch (response.status) {
            case 'OK':
                return response.candidates[0];
            case 'ZERO_RESULTS':
                throw new ArgumentParsingError(`Could not find '${val}' on Google Maps.`);
            default:
                throw new ArgumentParsingError(`Something went wrong when contacting Google Maps.`);
        }
    }
}

module.exports = GooglePlaceArgumentType;