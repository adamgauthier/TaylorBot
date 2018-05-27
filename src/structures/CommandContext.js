'use strict';

class CommandContext {
    constructor(messageContext, command) {
        this.messageContext = messageContext;
        this.command = command;

        this.args = this.command.command.args.map(info => {
            const type = this.messageContext.client.master.registry.types.getType(info.type);
            const canBeEmpty = type.canBeEmpty(messageContext, info);

            return {
                info,
                type,
                canBeEmpty
            };
        });
    }

    usage() {
        const keyword = this.messageContext.isGuild ?
            `${this.messageContext.guildSettings.prefix}${this.command.name}` :
            this.command.name;

        const args = this.args.map(({ info, canBeEmpty }) => {
            const label = canBeEmpty ? `${info.label}?` : info.label;
            return `<${label}>`;
        });

        return [keyword, ...args].join(' ');
    }
}

module.exports = CommandContext;