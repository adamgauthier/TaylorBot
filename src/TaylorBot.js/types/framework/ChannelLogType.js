'use strict';

const WordArgumentType = require('../base/Word.js');
const ArgumentParsingError = require('../ArgumentParsingError.js');

class ChannelLogTypeArgumentType extends WordArgumentType {
    get id() {
        return 'channel-log-type';
    }

    parse(val) {
        const word = super.parse(val);
        const clean = word.trim().toLowerCase();

        switch (clean) {
            case 'member':
                return 'member';
            case 'message':
                return 'message';
            default:
                throw new ArgumentParsingError(`Log type must be 'member' or 'message'.`);
        }
    }
}

module.exports = ChannelLogTypeArgumentType;
