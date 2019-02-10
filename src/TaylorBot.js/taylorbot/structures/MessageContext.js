'use strict';

class MessageContext {
    constructor(message, client) {
        this.message = message;
        this.client = client;
    }

    get isGuild() {
        return this.message.channel.type === 'text';
    }

    get guildSettings() {
        return this.client.master.registry.guilds.get(this.message.guild.id);
    }

    get prefix() {
        return this.isGuild ?
            this.guildSettings.prefix : '';
    }
}

module.exports = MessageContext;