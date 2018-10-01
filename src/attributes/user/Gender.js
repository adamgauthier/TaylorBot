'use strict';

const TextUserAttribute = require('../TextUserAttribute.js');

class GenderAttribute extends TextUserAttribute {
    constructor() {
        super({
            id: 'gender',
            description: 'gender',
            value: {
                label: 'gender',
                type: 'gender',
                example: 'female'
            }
        });
    }
}

module.exports = GenderAttribute;