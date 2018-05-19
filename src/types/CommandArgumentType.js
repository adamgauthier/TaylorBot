'use strict';

const { GlobalPaths } = require('globalobjects');

const ArgumentType = require(GlobalPaths.ArgumentType);
const ArgumentParsingError = require(GlobalPaths.ArgumentParsingError);

class CommandArgumentType extends ArgumentType {
    constructor() {
        super('command');
    }

    parse(val, message) {
        for (const command of message.client.oldRegistry.commands.values()) {
            if (command.name.toLowerCase() === val.toLowerCase())
                return command;
        }

        throw new ArgumentParsingError(`Command '${val.toLowerCase()}' doesn't exist.`);
    }
}

module.exports = CommandArgumentType;