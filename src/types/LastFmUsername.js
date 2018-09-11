'use strict';

const WordArgumentType = require('./Word.js');
const ArgumentParsingError = require('../structures/ArgumentParsingError.js');

const usernameRegex = '([a-z][a-z0-9_-]{1,14})';

class LastFmUsernameArgumentType extends WordArgumentType {
    get id() {
        return 'last-fm-username';
    }

    parse(val) {
        const lastFm = val.trim();
        const matches = lastFm.match(new RegExp(`^${usernameRegex}$`, 'i'));
        if (matches) {
            return matches[1];
        }
        else {
            try {
                const url = new URL(lastFm);
                if (url.hostname === 'www.last.fm') {
                    const matches = url.pathname.match(new RegExp(`^\\/user\\/${usernameRegex}(\\/.*)?$`, 'i'));
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

module.exports = LastFmUsernameArgumentType;