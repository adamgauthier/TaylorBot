'use strict';

const { URL } = require('url');
const EmbedPageEditor = require('./EmbedPageEditor.js');

class ImageResultsPageEditor extends EmbedPageEditor {
    update(pages, currentPage) {
        const imageResult = pages[currentPage];

        const imageURL =
        ['http:', 'https:'].includes(new URL(imageResult.link).protocol) ?
            imageResult.link : imageResult.image.thumbnailLink;

        this.embed
            .setTitle(imageResult.title)
            .setURL(imageResult.image.contextLink)
            .setImage(imageURL);
    }
}

module.exports = ImageResultsPageEditor;