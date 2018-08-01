'use strict';

const ArrayPageMessage = require('./ArrayPageMessage.js');

class ArrayDescriptionPageMessage extends ArrayPageMessage {
    update() {
        this.embed.setDescription(this.pages[this.currentPage]);
        this.embed.setFooter(`Page ${this.currentPage + 1}/${this.pages.length}`);
    }
}

module.exports = ArrayDescriptionPageMessage;