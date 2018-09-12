'use strict';

const SimpleTextUserAttribute = require('../SimpleTextUserAttribute.js');

class TumblrAttribute extends SimpleTextUserAttribute {
    constructor() {
        super({
            id: 'tumblr',
            description: 'Tumblr username',
            value: {
                label: 'username',
                type: 'tumblr-username',
                example: 'taylorswift'
            },
            canList: true
        });
    }

    format(attribute) {
        return `[${attribute}](https://${attribute}.tumblr.com/)`;
    }
}

module.exports = TumblrAttribute;