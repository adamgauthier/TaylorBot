'use strict';

const TextArgumentType = require('./Text.js');

const males = [
    'm', 'male', 'boy', 'man'
];
const females = [
    'f', 'female', 'girl', 'woman'
];

class GenderArgumentType extends TextArgumentType {
    get id() {
        return 'gender';
    }

    parse(val) {
        const gender = val.trim().toLowerCase();

        if (males.includes(gender))
            return 'Male';

        if (females.includes(gender))
            return 'Female';

        return 'Other';
    }
}

module.exports = GenderArgumentType;