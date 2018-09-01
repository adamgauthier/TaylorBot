'use strict';

const SimpleUserTextAttribute = require('../SimpleUserTextAttribute.js');

class BaeAttribute extends SimpleUserTextAttribute {
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