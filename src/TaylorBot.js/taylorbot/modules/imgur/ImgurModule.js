'use strict';

const fetch = require('node-fetch');
const { URLSearchParams } = require('url');

const { imgurClientID } = require('../../config/imgur.json');

class ImgurModule {
    static async upload(url) {
        const response = await fetch('https://api.imgur.com/3/image', {
            method: 'POST',
            headers: { 'Authorization': `Client-ID ${imgurClientID}` },
            body: new URLSearchParams({ image: url.toString() })
        }).then(res => res.json());

        return response;
    }
}

module.exports = ImgurModule;