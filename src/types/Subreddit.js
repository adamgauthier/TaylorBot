'use strict';

const WordArgumentType = require('./Word.js');
const ArgumentParsingError = require('../structures/ArgumentParsingError.js');

class SubredditArgumentType extends WordArgumentType {
    get id() {
        return 'subreddit';
    }

    parse(val) {
        const subreddit = val.trim();
        const matches = subreddit.match(/^(?:\/?r\/)?([A-Za-z0-9][A-Za-z0-9_]{2,20})$/);
        if (matches) {
            return matches[1];
        }

        throw new ArgumentParsingError(`Could not parse '${val}' into a valid subreddit name.`);
    }
}

module.exports = SubredditArgumentType;