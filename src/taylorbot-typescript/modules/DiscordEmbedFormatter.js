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
            .setAuthor(`${guild.name} ${guild.verified ? '✅' : ''} ${isVip ? '⭐' : ''}`, iconURL, iconURL)
            .setColor(guild.roles.highest.color);
    }

    static getIconURL(guild) {
        return guild.iconURL({ format: 'png' });
    }

    static guild(guild) {
        const iconURL = DiscordEmbedFormatter.getIconURL(guild);

        const { channels, roles, owner, region, createdTimestamp, memberCount } = guild;
        const categories = channels.filter(c => c.type === 'category');
        const textChannels = channels.filter(c => c.type === 'text');
        const voiceChannels = channels.filter(c => c.type === 'voice');

        const embed = DiscordEmbedFormatter.baseGuildHeader(guild)
            .addField('ID', `\`${guild.id}\``, true)
            .addField('Owner', owner.toString(), true)
            .addField('Members', `\`${memberCount}\``, true)
            .addField('Region', region, true)
            .addField('Created', TimeUtil.formatFull(createdTimestamp))
            .addField(
                StringUtil.plural(channels.size, 'Channel'),
                `${StringUtil.plural(categories.size, 'Category', '`')}, \`${textChannels.size}\` Text, \`${voiceChannels.size}\` Voice`)
            .addField(StringUtil.plural(roles.size, 'Role'), StringUtil.shrinkString(roles.array().join(', '), 75, ', ...', [',']));

        if (iconURL) {
            embed.setThumbnail(iconURL);
            embed.setURL(iconURL);
        }

        return embed;
    }
}

module.exports = DiscordEmbedFormatter;
