'use strict';

const EmbedPageEditor = require('./EmbedPageEditor.js');

class EmbedDescriptionPageEditor extends EmbedPageEditor {
    async update(pages, currentPage) {
        if (pages.length > 0) {
            this.embed.setDescription(
                await this.formatDescription(pages[currentPage])
            );
            this.embed.setFooter(`Page ${currentPage + 1}/${pages.length}`);
        }
        else {
            this.embed.setDescription('No data');
        }
    }

    formatDescription(currentPage) {
        return currentPage;
    }
}

module.exports = EmbedDescriptionPageEditor;