'use strict';

const UrlArgumentType = require('./Url.js');

class UrlOrAttachmentArgumentType extends UrlArgumentType {
    get id() {
        return 'url-or-attachment';
    }

    canBeEmpty({ message }) {
        return message.attachments.size > 0;
    }

    default({ message }) {
        return message.attachments.first().url;
    }
}

module.exports = UrlOrAttachmentArgumentType;