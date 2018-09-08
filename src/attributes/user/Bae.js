'use strict';

const SimpleTextUserAttribute = require('../SimpleTextUserAttribute.js');

class BaeAttribute extends SimpleTextUserAttribute {
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