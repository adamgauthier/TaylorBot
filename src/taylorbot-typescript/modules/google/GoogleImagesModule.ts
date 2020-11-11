import fetch = require('node-fetch');
import querystring = require('querystring');

import { EnvUtil } from '../util/EnvUtil';

const googleAPIKey = EnvUtil.getRequiredEnvVariable('TaylorBot_Google__ApiKey');
const customSearchId = EnvUtil.getRequiredEnvVariable('TaylorBot_Google__CustomSearchId');

export type CustomSearchItem = {
    title: string;
    link: string;
    image: { thumbnailLink: string; contextLink: string };
};

type CustomSearchResponse = {
    error: Record<string, any> | undefined;
    searchInformation: {
        formattedSearchTime: string;
        totalResults: string;
        formattedTotalResults: string;
    };
    items: CustomSearchItem[] | undefined;
};

export class GoogleImagesModule {
    static async search(searchText: string, numberOfResults: number): Promise<CustomSearchResponse> {
        return (await fetch(`https://www.googleapis.com/customsearch/v1?${querystring.stringify({
            'key': googleAPIKey,
            'cx': customSearchId,
            'safe': 'high',
            'num': numberOfResults,
            'searchType': 'image',
            'q': searchText
        })}`).then(res => res.json())) as CustomSearchResponse;
    }
}
