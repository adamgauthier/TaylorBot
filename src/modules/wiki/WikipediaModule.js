'use strict';

const fetch = require('node-fetch');
const querystring = require('querystring');

const DiscordEmbedFormatter = require('../DiscordEmbedFormatter.js');
const StringUtil = require('../StringUtil.js');

class WikipediaModule {
    static async getPage(search) {
        const response = await fetch(`https://en.wikipedia.org/w/api.php?${querystring.stringify({
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
        })}`).then(res => res.json());

        return response.query.pages[0];
    }

    static getPageEmbed(user, page) {
        const embed = DiscordEmbedFormatter
            .baseUserEmbed(user)
            .setTitle(page.title)
            .setURL(page.fullurl)
            .setDescription(StringUtil.shrinkString(page.extract, 250, ' (...)'))
            .setFooter(`${Object.values(page.pageviews)[0]} views in the last day`);

        if (page.original)
            embed.setImage(page.original.source);

        return embed;
    }
}

module.exports = WikipediaModule;