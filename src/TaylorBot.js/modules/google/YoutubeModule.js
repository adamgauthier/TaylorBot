'use strict';

const fetch = require('node-fetch');
const querystring = require('querystring');

const { googleAPIKey } = require('../../config/google.json');

class YoutubeModule {
    static async searchVideo(query) {
        const response = await fetch(`https://www.googleapis.com/youtube/v3/search?${querystring.stringify({
            'key': googleAPIKey,
            'part': 'snippet',
            'type': 'video',
            'q': query
        })}`).then(res => res.json());

        return response.items;
    }
}

module.exports = YoutubeModule;
