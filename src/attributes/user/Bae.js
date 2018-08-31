'use strict';

const SimpleUserTextAttribute = require('../SimpleUserTextAttribute.js');

class BaeAttribute extends SimpleUserTextAttribute {
    constructor() {
        super({
            id: 'bae',
            description: 'bae'
        });
    }
}

module.exports = BaeAttribute;