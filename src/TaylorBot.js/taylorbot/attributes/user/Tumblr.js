'use strict';

const TextUserAttribute = require('../TextUserAttribute.js');

class TumblrAttribute extends TextUserAttribute {
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
        const value = super.formatValue(attribute);
        return `[${value}](https://${value}.tumblr.com/)`;
    }
}

module.exports = TumblrAttribute;