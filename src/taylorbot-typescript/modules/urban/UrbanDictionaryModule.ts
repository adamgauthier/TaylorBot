import querystring = require('querystring');

export type UrbanResult = {
    definition: string;
    permalink: string;
    thumbs_up: number;
    author: string;
    word: string;
    written_on: string;
    thumbs_down: number;
};

type UrbanResponse = {
    list: UrbanResult[];
}

export class UrbanDictionaryModule {
    static async search(term: string): Promise<UrbanResult[]> {
        const response = (await fetch(`https://api.urbandictionary.com/v0/define?${querystring.stringify({
            term
        })}`).then(res => res.json())) as UrbanResponse;

        return response.list;
    }
}
