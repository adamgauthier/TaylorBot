'use strict';

const TextArgumentType = require('./Text.js');
const ArgumentParsingError = require('../structures/ArgumentParsingError.js');
const StringUtil = require('../modules/StringUtil.js');

const MIN_OPTIONS = 2;
const MAX_LENGTH = 2048;

class PollOptionsArgumentType extends TextArgumentType {
    get id() {
        return 'poll-options';
    }

    async parse(val) {
        const options = val.split(',').map(o => o.trim()).filter(o => o !== '');

        if (options.length < MIN_OPTIONS) {
            throw new ArgumentParsingError(
                `Can't start a poll with less than ${StringUtil.plural(MIN_OPTIONS, 'option')}, found ${StringUtil.plural(options.length, 'option')}.`
            );
        }

        const totalLength = options.reduce((acc, o) => acc + o.length + 20, 0);

        if (totalLength > MAX_LENGTH) {
            throw new ArgumentParsingError(`Can't start a poll with options that long, please reduce the number of options or their length.`);
        }

        return options;
    }
}

module.exports = PollOptionsArgumentType;