'use strict';

const ArrayPageEmbedMessage = require('./ArrayPageEmbedMessage.js');

class ArrayDescriptionPageMessage extends ArrayPageEmbedMessage {
    update() {
        this.embed.setDescription(this.pages[this.currentPage]);
        this.embed.setFooter(`Page ${this.currentPage + 1}/${this.pages.length}`);
    }
}

module.exports = ArrayDescriptionPageMessage;