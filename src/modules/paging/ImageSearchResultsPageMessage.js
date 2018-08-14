'use strict';

const ArrayPageEmbedMessage = require('./ArrayPageEmbedMessage.js');

class ImageSearchResultsPageMessage extends ArrayPageEmbedMessage {
    update() {
        const imageResult = this.pages[this.currentPage];

        this.embed
            .setTitle(imageResult.title)
            .setURL(imageResult.image.contextLink)
            .setImage(imageResult.link);
    }
}

module.exports = ImageSearchResultsPageMessage;