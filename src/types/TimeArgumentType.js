'use strict';

const chrono = require('chrono-node');
const { Paths } = require('globalobjects');

const ArgumentType = require(Paths.ArgumentType);
const ArgumentParsingError = require('../structures/ArgumentParsingError.js');

class TimeArgumentType extends ArgumentType {
    constructor() {
        super({
            includesSpaces: true
        });
    }

    get id() {
        return 'time';
    }

    parse(val) {
        const date = chrono.parseDate(val);

        if (date === null)
            throw new ArgumentParsingError(`Could not parse date from '${val}'.`);

        return date;
    }
}

module.exports = TimeArgumentType;