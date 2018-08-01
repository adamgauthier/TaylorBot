'use strict';

const ArrayPageMessage = require('./ArrayPageMessage.js');

class ImageSearchResultsPageMessage extends ArrayPageMessage {
    update() {
        const imageResult = this.pages[this.currentPage];

        this.embed
            .setTitle(imageResult.title)
            .setURL(imageResult.image.contextLink)
            .setImage(imageResult.link);
    }
}

module.exports = ImageSearchResultsPageMessage;