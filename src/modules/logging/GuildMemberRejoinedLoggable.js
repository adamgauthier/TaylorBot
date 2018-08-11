'use strict';

const { MessageEmbed } = require('discord.js');

const Loggable = require('./Loggable.js');
const DiscordEmbedFormatter = require('../DiscordEmbedFormatter.js');
const TimeUtil = require('../TimeUtil.js');

class GuildMemberRejoinedLoggable extends Loggable {
    constructor(member, firstJoinedAt) {
        super();
        this.member = member;
        this.firstJoinedAt = firstJoinedAt;
    }

    toEmbed() {
        const { user, joinedAt } = this.member;
        const avatarURL = DiscordEmbedFormatter.getAvatarURL(user);

        return new MessageEmbed()
            .setAuthor(`${user.tag} (${user.id})`, avatarURL, avatarURL)
            .setColor('#009c1a')
            .setDescription(this.firstJoinedAt !== undefined ?
                `\`❕\` This user first joined on ${TimeUtil.formatFull(this.firstJoinedAt)}.` :
                `\`❕\` I don't know when this user first joined.`
            )
            .setFooter('User rejoined')
            .setTimestamp(joinedAt);
    }
}

module.exports = GuildMemberRejoinedLoggable;