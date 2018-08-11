'use strict';

const fetch = require('node-fetch');
const querystring = require('querystring');

const DiscordEmbedFormatter = require('../DiscordEmbedFormatter.js');
const StringUtil = require('../StringUtil.js');

class UrbanDictionaryModule {
    static async search(term) {
        const response = await fetch(`http://api.urbandictionary.com/v0/define?${querystring.stringify({
            term
        })}`).then(res => res.json());

        return response.list;
    }

    static getResultEmbed(user, result) {
        return DiscordEmbedFormatter
            .baseUserEmbed(user)
            .setTitle(result.word)
            .setURL(result.permalink)
            .setTimestamp(new Date(Date.parse(result.written_on)))
            .setDescription(StringUtil.shrinkString(result.definition, 2048, ' (...)'))
            .setFooter(result.author)
            .addField('Votes', `👍 \`${result.thumbs_up}\` | \`${result.thumbs_down}\` 👎`, true);
    }
}

module.exports = UrbanDictionaryModule;