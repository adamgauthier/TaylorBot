'use strict';

const TextUserAttribute = require('../TextUserAttribute.js');

class SnapchatAttribute extends TextUserAttribute {
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
        const value = super.formatValue(attribute);
        return `[${value}](https://www.snapchat.com/add/${value})`;
    }
}

module.exports = SnapchatAttribute;