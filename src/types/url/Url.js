'use strict';

const { URL } = require('url');

const TextArgumentType = require('../base/Text.js');
const ArgumentParsingError = require('../ArgumentParsingError.js');

class UrlArgumentType extends TextArgumentType {
    get id() {
        return 'url';
    }

    parse(val) {
        try {
            return new URL(val);
        }
        catch (e) {
            if (e instanceof TypeError) {
                throw new ArgumentParsingError(`Could not parse a valid URL from '${val}'`);
            }
            else {
                throw e;
            }
        }
    }
}

module.exports = UrlArgumentType;