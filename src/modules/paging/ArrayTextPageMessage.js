'use strict';

const ArrayPageMessage = require('./ArrayPageMessage.js');

class ArrayTextPageMessage extends ArrayPageMessage {
    sendMessage(channel) {
        return this.client.sendMessage(channel, this.text);
    }

    update() {
        this.text = this.pages[this.currentPage];
    }

    edit() {
        return this.message.edit(this.text);
    }
}

module.exports = ArrayTextPageMessage;