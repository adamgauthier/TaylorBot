import fetch = require('node-fetch');
import querystring = require('querystring');

import { googleAPIKey } from '../../config/google.json';

export type PlaceCandidate = {
    formatted_address: string;
    geometry: {
        location: {
            lat: number;
            lng: number;
        };
    };
};

type PlaceResponse = {
    status: string;
    candidates: PlaceCandidate[];
};

export class GooglePlacesModule {
    static async findPlaceFromText(input: string): Promise<PlaceResponse> {
        return (await fetch(`https://maps.googleapis.com/maps/api/place/findplacefromtext/json?${querystring.stringify({
            key: googleAPIKey,
            inputtype: 'textquery',
            fields: 'formatted_address,geometry/location',
            input
        })}`).then(res => res.json())) as PlaceResponse;
    }
}
