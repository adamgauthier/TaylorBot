'use strict';

class MessageContext {
    constructor(message, client) {
        this.message = message;
        this.client = client;
        this.prefix = '';
        this.wasOnGoingCommandAdded = false;
    }

    get isGuild() {
        return this.message.channel.type === 'text';
    }

    get guildSettings() {
        return this.client.master.registry.guilds.get(this.message.guild.id);
    }
}

module.exports = MessageContext;
