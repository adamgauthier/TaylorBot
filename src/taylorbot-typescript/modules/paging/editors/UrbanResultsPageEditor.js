'use strict';

const EmbedPageEditor = require('./EmbedPageEditor.js');
const StringUtil = require('../../StringUtil.js');

class UrbanResultsPageEditor extends EmbedPageEditor {
    update(pages, currentPage) {
        const result = pages[currentPage];

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

module.exports = UrbanResultsPageEditor;