'use strict';

const TextUserAttribute = require('../TextUserAttribute.js');

class BaeAttribute extends TextUserAttribute {
    constructor() {
        super({
            id: 'bae',
            description: 'bae',
            value: {
                label: 'bae',
                type: 'text',
                example: 'Taylor Swift'
            }
        });
    }
}

module.exports = BaeAttribute;