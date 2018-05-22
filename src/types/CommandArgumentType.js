'use strict';

const { Paths } = require('globalobjects');

const ArgumentType = require(Paths.ArgumentType);
const ArgumentParsingError = require(Paths.ArgumentParsingError);

class CommandArgumentType extends ArgumentType {
    constructor() {
        super('command');
    }

    parse(val, message) {
        const command = message.client.master.registry.commands.resolve(val);

        if (command)
            return command;

        throw new ArgumentParsingError(`Command '${val}' doesn't exist.`);
    }
}

module.exports = CommandArgumentType;