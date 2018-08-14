'use strict';

const ArrayPageEmbedMessage = require('./ArrayPageEmbedMessage.js');
const StringUtil = require('../../modules/StringUtil.js');

class UrbanDictionaryResultsPageMessage extends ArrayPageEmbedMessage {
    update() {
        const result = this.pages[this.currentPage];

        this.embed.fields.pop();

        this.embed
            .setTitle(result.word)
            .setURL(result.permalink)
            .setTimestamp(new Date(Date.parse(result.written_on)))
            .setDescription(StringUtil.shrinkString(result.definition, 2048, ' (...)'))
            .setFooter(result.author)
            .addField('Votes', `👍 \`${result.thumbs_up}\` | \`${result.thumbs_down}\` 👎`, true);
    }
}

module.exports = UrbanDictionaryResultsPageMessage;