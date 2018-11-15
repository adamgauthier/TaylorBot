'use strict';

const WordArgumentType = require('../base/Word.js');
const ArgumentParsingError = require('../ArgumentParsingError.js');

class CommandArgumentType extends WordArgumentType {
    get id() {
        return 'command';
    }

    parse(val, { client }) {
        const command = client.master.registry.commands.resolve(val.trim());

        if (command)
            return command;

        throw new ArgumentParsingError(`Command '${val}' doesn't exist.`);
    }
}

module.exports = CommandArgumentType;