'use strict';

const { Paths } = require('globalobjects');

const ArgumentType = require(Paths.ArgumentType);
const ArgumentParsingError = require(Paths.ArgumentParsingError);

class CommandArgumentType extends ArgumentType {
    constructor() {
        super('command');
    }

    parse(val, message) {
        for (const command of message.client.master.registry.commands.values()) {
            if (command.name.toLowerCase() === val.toLowerCase())
                return command;
        }

        throw new ArgumentParsingError(`Command '${val.toLowerCase()}' doesn't exist.`);
    }
}

module.exports = CommandArgumentType;