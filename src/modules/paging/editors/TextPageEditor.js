'use strict';

const PageEditor = require('./PageEditor.js');

class TextPageEditor extends PageEditor {
    constructor() {
        super();
        this.text = null;
    }

    sendMessage(client, channel) {
        return client.sendMessage(channel, this.text);
    }

    update(pages, currentPage) {
        this.text = pages[currentPage];
    }

    edit(message) {
        return message.edit(this.text);
    }
}

module.exports = TextPageEditor;