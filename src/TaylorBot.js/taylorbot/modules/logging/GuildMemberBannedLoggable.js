'use strict';

const { MessageEmbed } = require('discord.js');

const Loggable = require('./Loggable.js');
const DiscordEmbedFormatter = require('../DiscordEmbedFormatter.js');

class GuildMemberBannedLoggable extends Loggable {
    constructor(user, bannedAt) {
        super();
        this.user = user;
        this.bannedAt = bannedAt;
    }

    toEmbed() {
        const avatarURL = DiscordEmbedFormatter.getAvatarURL(this.user);

        return new MessageEmbed()
            .setAuthor(`${this.user.tag} (${this.user.id})`, avatarURL, avatarURL)
            .setColor('#ff5252')
            .setFooter('User was banned')
            .setTimestamp(this.bannedAt);
    }
}

module.exports = GuildMemberBannedLoggable;