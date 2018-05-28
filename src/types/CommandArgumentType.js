'use strict';

const { Paths } = require('globalobjects');

const ArgumentType = require(Paths.ArgumentType);
const ArgumentParsingError = require(Paths.ArgumentParsingError);

class CommandArgumentType extends ArgumentType {
    get id() {
        return 'command';
    }

    parse(val, { client }) {
        const command = client.master.registry.commands.resolve(val);

        if (command)
            return command;

        throw new ArgumentParsingError(`Command '${val}' doesn't exist.`);
    }
}

module.exports = CommandArgumentType;