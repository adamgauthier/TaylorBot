'use strict';

const WordArgumentType = require('./Word.js');
const ArgumentParsingError = require('../structures/ArgumentParsingError.js');

class UrlArgumentType extends WordArgumentType {
    get id() {
        return 'last-fm-username';
    }

    parse(val) {
        const lastFm = val.trim();
        const matches = lastFm.match(/^[a-z][a-z0-9_-]{1,14}$/i);
        if (matches) {
            return matches[0];
        }
        else {
            try {
                const url = new URL(lastFm);
                if (url.hostname === 'www.last.fm') {
                    const matches = url.pathname.match(/^\/user\/([a-z][a-z0-9_-]{1,14})$/i);
                    if (matches)
                        return matches[1];
                }
            }
            catch (e) {
                // Continue on error, it's not a URL
            }
        }

        throw new ArgumentParsingError(`Could not parse '${val}' into a valid Last.fm username.`);
    }
}

module.exports = UrlArgumentType;