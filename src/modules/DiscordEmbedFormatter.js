'use strict';

const { MessageEmbed } = require('discord.js');
const { GlobalPaths } = require('globalobjects');

const TimeUtil = require(GlobalPaths.TimeUtil);
const StringUtil = require(GlobalPaths.StringUtil);

class DiscordEmbedFormatter {
    static baseUserHeader(user, avatarURL = DiscordEmbedFormatter.getAvatarURL(user)) {
        return new MessageEmbed()
            .setAuthor(`${user.tag} ${(user.bot ? '🤖' : '')}`, avatarURL, avatarURL)
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

    static member(member) {
        const { user, client } = member;
        const { presence } = user;

        const avatarURL = DiscordEmbedFormatter.getAvatarURL(user);

        const shared = client.guilds.filterArray(g =>
            g.members.exists('id', member.id)
        ).map(g => g.name);

        const roles = member.roles.map(r => r.name);

        const embed = DiscordEmbedFormatter.baseUserHeader(user, avatarURL)
            .setURL(avatarURL)
            .setThumbnail(avatarURL)
            .addField('ID', `\`${user.id}\``, true);

        if (presence.activity)
            embed.addField('Activity', presence.activity.name, true);

        // TODO: Timezones
        embed
            .addField('Server Joined', TimeUtil.formatFull(member.joinedTimestamp))
            .addField('Account Created', TimeUtil.formatFull(user.createdTimestamp))
            .addField(StringUtil.plural(roles.length, 'Role'), StringUtil.shrinkString(roles.join(', '), 75, ', ...', [',']))
            .addField(`Shares ${StringUtil.plural(shared.length, 'Server')}`, StringUtil.shrinkString(shared.join(', '), 75, ', ...', [',']));

        return embed;
    }

    static guild(guild) {
        const iconURL = DiscordEmbedFormatter.getIconURL(guild);

        const { channels, roles, owner, region, createdTimestamp, memberCount, presences } = guild;
        const categories = channels.findAll('type', 'category');
        const textChannels = channels.findAll('type', 'text');
        const voiceChannels = channels.findAll('type', 'voice');

        const embed = DiscordEmbedFormatter.baseGuildHeader(guild)
            .addField('ID', `\`${guild.id}\``, true)
            .addField('Owner', owner.toString(), true)
            .addField(StringUtil.plural(memberCount, 'Member'), `\`${presences.findAll('status', 'online').length}\` Online`, true)
            .addField('Region', region, true)
            .addField('Created', TimeUtil.formatFull(createdTimestamp))
            .addField(
                StringUtil.plural(channels.size, 'Channel'),
                `${StringUtil.plural(categories.length, 'Category', '`')}, \`${textChannels.length}\` Text, \`${voiceChannels.length}\` Voice`)
            .addField(StringUtil.plural(roles.size, 'Role'), StringUtil.shrinkString(roles.array().join(', '), 75, ', ...', [',']));

        if (iconURL) {
            embed.setThumbnail(iconURL);
            embed.setURL(iconURL);
        }

        return embed;
    }
}

module.exports = DiscordEmbedFormatter;