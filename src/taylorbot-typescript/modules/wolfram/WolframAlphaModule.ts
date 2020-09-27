import fetch = require('node-fetch');
import querystring = require('querystring');

import { wolframAppId } from '../../config/wolfram.json';

type WolframQueryResult = {
    success: boolean;
    error: boolean;
    timing: number;
    numpods: number;
    pods: { subpods: { plaintext: string; img: { src: string } }[] }[];
};

type WolframResponse = {
    queryresult: WolframQueryResult;
};

export class WolframAlphaModule {
    static async query(input: string): Promise<WolframQueryResult> {
        const response = (await fetch(`https://api.wolframalpha.com/v2/query?${querystring.stringify({
            input,
            appid: wolframAppId,
            output: 'json',
            ip: '192.168.1.1',
            podindex: '1,2'
        })}`).then(res => res.json())) as WolframResponse;

        return response.queryresult;
    }
}
