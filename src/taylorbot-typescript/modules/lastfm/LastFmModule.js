'use strict';

const fetch = require('node-fetch');
const querystring = require('querystring');

const { API_KEY } = require('../../config/lastfm.json');

class LastFmModule {
    static getRecentTracks(username, limit = 10) {
        return fetch(`https://ws.audioscrobbler.com/2.0/?${querystring.stringify({
            method: 'user.getrecenttracks',
            user: username,
            limit,
            api_key: API_KEY,
            format: 'json',
            extended: 1
        })}`).then(res => res.json());
    }
}

module.exports = LastFmModule;