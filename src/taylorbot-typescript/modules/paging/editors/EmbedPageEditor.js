'use strict';

const PageEditor = require('./PageEditor.js');

class EmbedPageEditor extends PageEditor {
    constructor(embed) {
        super();
        if (new.target === EmbedPageEditor) {
            throw new Error(`Can't instantiate abstract ${this.constructor.name} class.`);
        }

        this.embed = embed;
    }

    sendMessage(client, channel) {
        return client.sendEmbed(channel, this.embed);
    }

    update(pages, currentPage) { // eslint-disable-line no-unused-vars
        throw new Error(`${this.constructor.name} doesn't have a ${this.update.name}() method.`);
    }

    edit(message) {
        return message.edit('', this.embed);
    }
}

module.exports = EmbedPageEditor;