import { MessageEmbed, User } from 'discord.js';
import querystring = require('querystring');

import { DiscordEmbedFormatter } from '../discord/DiscordEmbedFormatter';
import { StringUtil } from '../util/StringUtil';

type WikipediaPage = {
    invalid: any;
    missing: any;
    title: string;
    fullurl: string;
    extract: string;
    original: { source: string } | undefined;
    pageviews: Record<string, number>;
};

export class WikipediaModule {
    static async getPage(search: string): Promise<WikipediaPage> {
        const response = (await fetch(`https://en.wikipedia.org/w/api.php?${querystring.stringify({
            'action': 'query',
            'format': 'json',
            'prop': 'extracts|info|pageimages|pageviews',
            'titles': search,
            'formatversion': '2',
            'exintro': '1',
            'explaintext': '1',
            'inprop': 'url',
            'piprop': 'original',
            'pvipdays': '1',
            'redirects': ''
        })}`).then(res => res.json())) as { query: { pages: WikipediaPage[] } };

        return response.query.pages[0];
    }

    static getPageEmbed(user: User, page: WikipediaPage): MessageEmbed {
        const embed = DiscordEmbedFormatter
            .baseUserSuccessEmbed(user)
            .setTitle(page.title)
            .setURL(page.fullurl)
            .setDescription(StringUtil.shrinkString(page.extract, 250, ' (...)'))
            .setFooter({ text: `${Object.values(page.pageviews)[0]} views in the last day` });

        if (page.original)
            embed.setImage(page.original.source);

        return embed;
    }
}
