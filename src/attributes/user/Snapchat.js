'use strict';

const SimpleTextUserAttribute = require('../SimpleTextUserAttribute.js');

class SnapchatAttribute extends SimpleTextUserAttribute {
    constructor() {
        super({
            id: 'snapchat',
            aliases: ['snap'],
            description: 'Snapchat username',
            value: {
                label: 'username',
                type: 'snapchat-username',
                example: 'taylorswift'
            },
            canList: true
        });
    }

    format(attribute) {
        return `[${attribute}](https://www.snapchat.com/add/${attribute})`;
    }
}

module.exports = SnapchatAttribute;