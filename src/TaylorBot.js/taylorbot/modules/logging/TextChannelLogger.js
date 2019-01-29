'use strict';

class TextChannelLogger {
    constructor(client) {
        this.client = client;
    }

    async log(guild, loggable) {
        const logChannels = await this.client.master.database.textChannels.getAllLogChannelsInGuild(guild);
        for (const logChannel of logChannels) {
            const channel = guild.channels.resolve(logChannel.channel_id);
            if (channel) {
                this.client.sendEmbed(channel, loggable.toEmbed());
            }
        }
    }
}

module.exports = TextChannelLogger;