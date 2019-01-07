'use strict';

const UserAttribute = require('../UserAttribute.js');
const SimplePresentor = require('../presentors/SimplePresentor.js');

class OldMinutesAttribute extends UserAttribute {
    constructor() {
        super({
            id: 'oldminutes',
            description: 'minutes spent online (before December 25th 2015)',
            presentor: SimplePresentor
        });
    }

    retrieve(database, user) {
        return database.integerAttributes.get(this.id, user);
    }

    formatValue(attribute) {
        return attribute.integer_value.toString();
    }
}

module.exports = OldMinutesAttribute;