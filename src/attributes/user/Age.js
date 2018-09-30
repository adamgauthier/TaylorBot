'use strict';

const IntegerUserAttribute = require('../IntegerUserAttribute.js');

class AgeAttribute extends IntegerUserAttribute {
    constructor() {
        super({
            id: 'age',
            description: 'age',
            value: {
                label: 'age',
                type: 'age',
                example: '22'
            }
        });
    }
}

module.exports = AgeAttribute;