'use strict';

const IntegerUserAttribute = require('../IntegerUserAttribute.js');

class OldMinutesAttribute extends IntegerUserAttribute {
    constructor() {
        super({
            id: 'oldminutes',
            description: 'minutes spent online (before December 25th 2015)'
        });
    }
}

module.exports = OldMinutesAttribute;