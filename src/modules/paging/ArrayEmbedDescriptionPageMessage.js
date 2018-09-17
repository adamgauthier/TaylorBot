'use strict';

const ArrayPageEmbedMessage = require('./ArrayPageEmbedMessage.js');

class ArrayEmbedDescriptionPageMessage extends ArrayPageEmbedMessage {
    async update() {
        if (this.pages.length > 0) {
            this.embed.setDescription(
                await this.formatDescription(this.pages[this.currentPage])
            );
            this.embed.setFooter(`Page ${this.currentPage + 1}/${this.pages.length}`);
        }
        else {
            this.embed.setDescription('No data');
        }
    }

    formatDescription(currentPage) {
        return currentPage;
    }
}

module.exports = ArrayEmbedDescriptionPageMessage;