'use strict';

const TextArgumentType = require('../base/Text.js');

class CommaSeparatedOptionsArgumentType extends TextArgumentType {
    get id() {
        return 'comma-separated-options';
    }

    parse(val) {
        return val.split(',').map(o => o.trim()).filter(o => o !== '');
    }
}

module.exports = CommaSeparatedOptionsArgumentType;