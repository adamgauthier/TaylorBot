'use strict';

const { URL } = require('url');
const ArrayPageEmbedMessage = require('./ArrayPageEmbedMessage.js');

class ImageSearchResultsPageMessage extends ArrayPageEmbedMessage {
    update() {
        const imageResult = this.pages[this.currentPage];

        const imageURL =
        ['http:', 'https:'].includes(new URL(imageResult.link).protocol) ?
            imageResult.link : imageResult.image.thumbnailLink;

        this.embed
            .setTitle(imageResult.title)
            .setURL(imageResult.image.contextLink)
            .setImage(imageURL);
    }
}

module.exports = ImageSearchResultsPageMessage;