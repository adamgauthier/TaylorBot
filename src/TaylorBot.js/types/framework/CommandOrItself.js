'use strict';

const CommandArgumentType = require('./Command.js');

class CommandOrItselfArgumentType extends CommandArgumentType {
    get id() {
        return 'command-or-itself';
    }

    canBeEmpty() {
        return true;
    }

    default({ command }) {
        return command;
    }
}

module.exports = CommandOrItselfArgumentType;