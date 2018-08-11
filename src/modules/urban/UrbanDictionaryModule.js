'use strict';

const fetch = require('node-fetch');
const querystring = require('querystring');

class UrbanDictionaryModule {
    static async search(term) {
        const response = await fetch(`http://api.urbandictionary.com/v0/define?${querystring.stringify({
            term
        })}`).then(res => res.json());

        return response.list;
    }
}

module.exports = UrbanDictionaryModule;