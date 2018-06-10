'use strict';

const chrono = require('chrono-node');
const { Paths } = require('globalobjects');

const ArgumentType = require(Paths.ArgumentType);
const ArgumentParsingError = require(Paths.ArgumentParsingError);

class FutureTimeArgumentType extends ArgumentType {
    constructor() {
        super({
            includesSpaces: true
        });
    }

    get id() {
        return 'future-time';
    }

    parse(val) {
        const date = chrono.parseDate(val, new Date(), { forwardDate: true });

        if (date === null)
            throw new ArgumentParsingError(`Could not parse future date from '${val}'.`);

        return date;
    }
}

module.exports = FutureTimeArgumentType;