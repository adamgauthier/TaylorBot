'use strict';

const ArrayPageEmbedMessage = require('./ArrayPageEmbedMessage.js');

class ArrayEmbedDescriptionPageMessage extends ArrayPageEmbedMessage {
    update() {
        if (this.pages.length > 0) {
            this.embed.setDescription(this.pages[this.currentPage]);
            this.embed.setFooter(`Page ${this.currentPage + 1}/${this.pages.length}`);
        }
        else {
            this.embed.setDescription('No data');
        }
    }
}

module.exports = ArrayEmbedDescriptionPageMessage;