'use strict';

const UnsafeRandomModule = require('../modules/random/UnsafeRandomModule.js');

class CommandMessageContext {
    constructor(messageContext, command) {
        this.messageContext = messageContext;
        this.command = command;

        this.args = this.command.command.args.map(info => {
            const type = this.client.master.registry.types.getType(info.type);
            const canBeEmpty = type.canBeEmpty(messageContext, info);

            return {
                info,
                mustBeQuoted: info.mustBeQuoted || type.mustBeQuoted,
                includesSpaces: info.includesSpaces || type.includesSpaces,
                includesNewLines: info.includesNewLines || type.includesNewLines,
                type,
                canBeEmpty
            };
        });
    }

    get client() {
        return this.messageContext.client;
    }

    get message() {
        return this.messageContext.message;
    }

    usage() {
        const keyword = `${this.messageContext.prefix}${this.command.name}`;

        const args = this.args.map(({ info, canBeEmpty, mustBeQuoted }) => {
            const label = canBeEmpty ? `${info.label}?` : info.label;
            return mustBeQuoted ? `'<${label}>'` : `<${label}>`;
        });

        return [keyword, ...args].join(' ');
    }

    argsUsage() {
        return this.args.map(({ info, canBeEmpty, mustBeQuoted }) => {
            const label = canBeEmpty ? `${info.label}?` : info.label;
            let hint = info.prompt;
            if (mustBeQuoted)
                hint += ' It must be surrounded by quotes!';
            if (canBeEmpty)
                hint += ' It can be empty!';

            return {
                identifier: mustBeQuoted ? `'<${label}>'` : `<${label}>`,
                hint
            };
        });
    }

    helpUsage() {
        const helpCommandName = 'help';
        const keyword = `${this.messageContext.prefix}${helpCommandName}`;

        return `${keyword} ${this.command.name}`;
    }

    example() {
        return `${this.messageContext.prefix}${this.command.command.name} ${UnsafeRandomModule.randomInArray(this.command.command.examples)}`;
    }
}

module.exports = CommandMessageContext;