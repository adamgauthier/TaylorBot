'use strict';

const fetch = require('node-fetch');
const querystring = require('querystring');
const { Paths } = require('globalobjects');

const { googleAPIKey, customsearchID } = require(Paths.GoogleConfig);

class GoogleImagesModule {
    static search(searchText) {
        return fetch(`https://www.googleapis.com/customsearch/v1?${querystring.stringify({
            'key': googleAPIKey,
            'cx': customsearchID,
            'safe': 'high',
            'num': 1,
            'searchType': 'image',
            'q': searchText
        })}`).then(res => res.json());
    }
}

module.exports = GoogleImagesModule;