'use strict';

const { MessageEmbed, GuildChannel, TextChannel, DMChannel, VoiceChannel, CategoryChannel } = require('discord.js');
const { Paths } = require('globalobjects');

const TimeUtil = require('../modules/TimeUtil.js');
const StringUtil = require(Paths.StringUtil);

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

    static member(member) {
        const { user, client } = member;
        const { presence } = user;

        const avatarURL = DiscordEmbedFormatter.getAvatarURL(user);

        const shared = client.guilds.filter(g =>
            g.members.has(member.id)
        ).map(g => g.name);

        const roles = member.roles.map(r => r.name);

        const embed = DiscordEmbedFormatter.baseUserEmbed(user, avatarURL)
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
        const categories = channels.filter(c => c.type === 'category');
        const textChannels = channels.filter(c => c.type === 'text');
        const voiceChannels = channels.filter(c => c.type === 'voice');

        const embed = DiscordEmbedFormatter.baseGuildHeader(guild)
            .addField('ID', `\`${guild.id}\``, true)
            .addField('Owner', owner.toString(), true)
            .addField(StringUtil.plural(memberCount, 'Member'), `\`${presences.filter(p => p.status === 'online').size}\` Online`, true)
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

    static channel(channel) {
        const embed = new MessageEmbed()
            .addField('ID', `\`${channel.id}\``, true)
            .addField('Type', channel.type, true)
            .addField('Created', TimeUtil.formatFull(channel.createdTimestamp));

        if (channel instanceof GuildChannel) {
            const { guild, parent } = channel;
            if (parent) {
                embed.addField('Category', `${parent.name} (\`${parent.id}\`)`, true);
            }

            embed
                .setAuthor(`${channel.name} ${channel.nsfw ? '🔞' : ''}`)
                .addField('Server', `${guild.name} (\`${guild.id}\`)`, true);

            if (channel instanceof TextChannel) {
                embed.addField('Topic', channel.topic ? channel.topic : '[Empty]');
            }
            else if (channel instanceof VoiceChannel) {
                embed
                    .addField('Bitrate', `${channel.bitrate} bps`, true)
                    .addField('User Limit', channel.userLimit, true);
            }
            else if (channel instanceof CategoryChannel) {
                const { children } = channel;
                if (children) {
                    embed
                        .addField(`${children.size} Children`, StringUtil.shrinkString(children.map(c => c.name).join(', '), 75, ', ...', [',']));
                }
            }
        }
        else if (channel instanceof DMChannel) {
            embed.addField('Recipient', channel.recipient.toString(), true);
        }

        return embed;
    }

    static role(role) {
        const { members, guild } = role;
        const embed = new MessageEmbed()
            .setColor(role.color)
            .setAuthor(role.name)
            .addField('ID', `\`${role.id}\``, true)
            .addField('Color', `\`${role.hexColor}\``, true)
            .addField('Server', `${guild.name} (\`${guild.id}\`)`, true)
            .addField('Created', TimeUtil.formatFull(role.createdTimestamp))
            .addField(StringUtil.plural(members.size, 'Member'), StringUtil.shrinkString(members.map(m => m.displayName).join(', '), 75, ', ...', [',']));

        return embed;
    }
}

module.exports = DiscordEmbedFormatter;