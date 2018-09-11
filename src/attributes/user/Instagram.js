'use strict';

const SimpleTextUserAttribute = require('../SimpleTextUserAttribute.js');

class InstagramAttribute extends SimpleTextUserAttribute {
    constructor() {
        super({
            id: 'instagram',
            aliases: ['insta'],
            description: 'Instagram username',
            value: {
                label: 'username',
                type: 'instagram-username',
                example: 'taylorswift'
            },
            canList: true
        });
    }

    format(attribute) {
        return `[${attribute}](https://www.instagram.com/${attribute}/)`;
    }
}

module.exports = InstagramAttribute;