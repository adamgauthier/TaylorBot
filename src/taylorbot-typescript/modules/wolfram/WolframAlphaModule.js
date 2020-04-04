'use strict';

const fetch = require('node-fetch');
const querystring = require('querystring');

const { wolframAppId } = require('../../config/wolfram.json');

class WolframAlphaModule {
    static async query(input) {
        const response = await fetch(`https://api.wolframalpha.com/v2/query?${querystring.stringify({
            input,
            appid: wolframAppId,
            output: 'json',
            ip: '192.168.1.1',
            podindex: '1,2'
        })}`).then(res => res.json());

        return response.queryresult;
    }
}

module.exports = WolframAlphaModule;