import querystring = require('querystring');

import { EnvUtil } from '../util/EnvUtil';

const googleAPIKey = EnvUtil.getRequiredEnvVariable('TaylorBot_Google__ApiKey');

type YoutubeSearchItem = {
    id: {
        videoId: string;
    };
};

type YoutubeSearchResponse = {
    items: YoutubeSearchItem[];
};

export class YoutubeModule {
    static async searchVideo(query: string): Promise<YoutubeSearchItem[]> {
        const response = (await fetch(`https://www.googleapis.com/youtube/v3/search?${querystring.stringify({
            'key': googleAPIKey,
            'part': 'snippet',
            'type': 'video',
            'q': query
        })}`).then(res => res.json())) as YoutubeSearchResponse;

        return response.items;
    }
}
