'use strict';

class PageEditor {
    constructor() {
        if (new.target === PageEditor) {
            throw new Error(`Can't instantiate abstract ${this.constructor.name} class.`);
        }
    }

    sendMessage(client, channel) { // eslint-disable-line no-unused-vars
        throw new Error(`${this.constructor.name} doesn't have a ${this.sendMessage.name}() method.`);
    }

    update(pages, currentPage) { // eslint-disable-line no-unused-vars
        throw new Error(`${this.constructor.name} doesn't have a ${this.update.name}() method.`);
    }

    edit(message) { // eslint-disable-line no-unused-vars
        throw new Error(`${this.constructor.name} doesn't have a ${this.edit.name}() method.`);
    }
}

module.exports = PageEditor;