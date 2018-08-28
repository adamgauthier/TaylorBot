'use strict';

const { URL } = require('url');

const WordArgumentType = require('./Word.js');
const ArgumentParsingError = require('../structures/ArgumentParsingError.js');

class UrlArgumentType extends WordArgumentType {
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