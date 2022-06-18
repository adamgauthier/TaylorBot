import { URL } from 'url';
import { URLSearchParams } from 'url';
import { EnvUtil } from '../util/EnvUtil';

const imgurClientID = EnvUtil.getRequiredEnvVariable('TaylorBot_Imgur__ClientId');

type ImgurUploadResponse = { success: boolean; data: { link: string } };

export class ImgurModule {
    static async upload(url: URL): Promise<ImgurUploadResponse> {
        const response = (await fetch('https://api.imgur.com/3/image', {
            method: 'POST',
            headers: { 'Authorization': `Client-ID ${imgurClientID}` },
            body: new URLSearchParams({ image: url.toString() })
        }).then(res => res.json())) as ImgurUploadResponse;

        return response;
    }
}
