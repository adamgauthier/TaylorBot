'use strict';

const rp = require('request-promise');
const { Paths } = require('globalobjects');

const { googleAPIKey, customsearchID } = require(Paths.GoogleConfig);

const rpOptions = {
    'uri': 'https://www.googleapis.com/customsearch/v1',
    'qs': {
        'key': googleAPIKey,
        'cx': customsearchID,
        'safe': 'high',
        'num': 1,
        'searchType': 'image'
    },
    'json': true
};

class GoogleImagesModule {
    static search(searchText) {
        const options = {
            ...rpOptions
        };
        options.qs.q = searchText;
        return rp(options);
    }
}

module.exports = GoogleImagesModule;