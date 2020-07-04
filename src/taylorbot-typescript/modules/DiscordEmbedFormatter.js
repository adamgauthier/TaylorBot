'use strict';

const { MessageEmbed } = require('discord.js');

const TimeUtil = require('../modules/TimeUtil.js');
const StringUtil = require('../modules/StringUtil.js');

class DiscordEmbedFormatter {
    static baseUserHeader(user, avatarURL = DiscordEmbedFormatter.getAvatarURL(user)) {
        return new MessageEmbed()
            .setAuthor(`${user.tag} ${(user.bot ? '🤖' : '')}`, avatarURL, avatarURL);
    }

    static baseUserEmbed(user, avatarURL = DiscordEmbedFormatter.getAvatarURL(user)) {
        return DiscordEmbedFormatter.baseUserHeader(user, avatarURL)
            .setColor(DiscordEmbedFormatter.getStatusColor(user.presence.status));
    }

    static getStatusColor(status) {
        switch (status) {
            case 'online':
                return '#43b581';
            case 'idle':
                return '#faa61a';
            case 'dnd':
                return '#f04747';
            case 'offline':
                return '#747f8d';
            default:
                return 'RANDOM';
        }
    }

    static getAvatarURL(user, size = 128) {
        return user.displayAvatarURL({
            format: user.avatar ? user.avatar.startsWith('a_') ? 'gif' : 'jpg' : undefined,
            size
        });
    }

    static baseGuildHeader(guild, iconURL = DiscordEmbedFormatter.getIconURL(guild)) {
        const isVip = guild.features.includes('VIP_REGIONS');

        return new MessageEmbed()
            .setAuthor(`${guild.name}${isVip ? ' ⭐' : ''}`, iconURL, iconURL)
            .setColor(guild.roles.highest.color);
    }

    static getIconURL(guild) {
        return guild.iconURL({ format: 'png' });
    }
}

module.exports = DiscordEmbedFormatter;
