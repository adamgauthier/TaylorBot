'use strict';

class CommandContext {
    constructor(messageContext, command) {
        this.messageContext = messageContext;
        this.command = command;
    }

    get keyword() {
        return this.messageContext.isGuild ?
            `${this.messageContext.guildSettings.prefix}${this.command.name}` :
            this.command.name;
    }
}

module.exports = CommandContext;