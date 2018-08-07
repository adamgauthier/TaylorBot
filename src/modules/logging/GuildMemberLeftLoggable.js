'use strict';

const { MessageEmbed } = require('discord.js');

const Loggable = require('./Loggable.js');
const DiscordEmbedFormatter = require('../DiscordEmbedFormatter.js');

class GuildMemberLeftLoggable extends Loggable {
    constructor(member, leftAt) {
        super();
        this.member = member;
        this.leftAt = leftAt;
    }

    toEmbed() {
        const { user } = this.member;
        const avatarURL = DiscordEmbedFormatter.getAvatarURL(user);

        return new MessageEmbed()
            .setAuthor(`${user.tag} (${user.id})`, avatarURL, avatarURL)
            .setColor('#fd6a02')
            .setFooter('User left')
            .setTimestamp(this.leftAt);
    }
}

module.exports = GuildMemberLeftLoggable;