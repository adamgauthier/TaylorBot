'use strict';

const { Paths } = require('globalobjects');

const MessageContext = require(Paths.MessageContext);

class CommandMessageContext extends MessageContext {
    constructor(messageContext, command) {
        super(messageContext.message, messageContext.client);
        this.command = command;

        this.args = this.command.command.args.map(info => {
            const type = this.client.master.registry.types.getType(info.type);
            const canBeEmpty = type.canBeEmpty(messageContext, info);

            return {
                info,
                type,
                canBeEmpty
            };
        });
    }

    usage() {
        const keyword = this.isGuild ?
            `${this.guildSettings.prefix}${this.command.name}` :
            this.command.name;

        const args = this.args.map(({ info, canBeEmpty }) => {
            const label = canBeEmpty ? `${info.label}?` : info.label;
            return `<${label}>`;
        });

        return [keyword, ...args].join(' ');
    }
}

module.exports = CommandMessageContext;