'use strict';

const MessageContext = require('../structures/MessageContext.js');
const ArrayUtil = require('../modules/ArrayUtil.js');

class CommandMessageContext extends MessageContext {
    constructor(messageContext, command) {
        super(messageContext.message, messageContext.client);
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

    usage() {
        const keyword = this.isGuild ?
            `${this.guildSettings.prefix}${this.command.name}` :
            this.command.name;

        const args = this.args.map(({ info, canBeEmpty, mustBeQuoted }) => {
            const label = canBeEmpty ? `${info.label}?` : info.label;
            return mustBeQuoted ? `'<${label}>'` : `<${label}>`;
        });

        return [keyword, ...args].join(' ');
    }

    example() {
        return this.guildSettings.prefix + ArrayUtil.random(this.command.command.examples);
    }
}

module.exports = CommandMessageContext;