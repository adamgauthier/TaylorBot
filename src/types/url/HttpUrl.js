'use strict';

const UrlArgumentType = require('./Url.js');
const ArgumentParsingError = require('../ArgumentParsingError.js');

class HttpUrlArgumentType extends UrlArgumentType {
    get id() {
        return 'http-url';
    }

    parse(val) {
        const url = super.parse(val);

        if (!['http:', 'https:'].includes(url.protocol)) {
            throw new ArgumentParsingError(`URL '${val}' must be of http or https protocol.`);
        }

        return url;
    }
}

module.exports = HttpUrlArgumentType;