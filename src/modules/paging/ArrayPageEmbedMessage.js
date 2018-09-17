'use strict';

const ArrayPageMessage = require('./ArrayPageMessage.js');

class ArrayPageEmbedMessage extends ArrayPageMessage {
    constructor(client, owner, pages, embed) {
        super(client, owner, pages);
        if (new.target === ArrayPageEmbedMessage) {
            throw new Error(`Can't instantiate abstract ${this.constructor.name} class.`);
        }

        this.embed = embed;
    }

    sendMessage(channel) {
        return this.client.sendEmbed(channel, this.embed);
    }

    update() {
        throw new Error(`${this.constructor.name} doesn't have an update() method.`);
    }

    edit() {
        return this.message.edit('', this.embed);
    }
}

module.exports = ArrayPageEmbedMessage;