'use strict';

const { MessageEmbed } = require('discord.js');

const Loggable = require('./Loggable.js');
const DiscordEmbedFormatter = require('../DiscordEmbedFormatter.js');

class GuildMemberUnbannedLoggable extends Loggable {
    constructor(user, unbannedAt) {
        super();
        this.user = user;
        this.unbannedAt = unbannedAt;
    }

    toEmbed() {
        const avatarURL = DiscordEmbedFormatter.getAvatarURL(this.user);

        return new MessageEmbed()
            .setAuthor(`${this.user.tag} (${this.user.id})`, avatarURL, avatarURL)
            .setColor('#be29ec')
            .setFooter('User was unbanned')
            .setTimestamp(this.unbannedAt);
    }
}

module.exports = GuildMemberUnbannedLoggable;