import { Guild, MessageEmbed, User } from 'discord.js';

export class DiscordEmbedFormatter {
    static baseUserHeader(user: User, avatarURL = DiscordEmbedFormatter.getAvatarURL(user)): MessageEmbed {
        return new MessageEmbed()
            .setAuthor(`${user.tag} ${(user.bot ? '🤖' : '')}`, avatarURL, avatarURL);
    }

    static baseUserEmbed(user: User, avatarURL = DiscordEmbedFormatter.getAvatarURL(user)): MessageEmbed {
        return DiscordEmbedFormatter.baseUserHeader(user, avatarURL)
            .setColor(DiscordEmbedFormatter.getStatusColor(user.presence.status));
    }

    static getStatusColor(status: string): string {
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

    static getAvatarURL(user: User, size: 128 | 16 | 32 | 64 | 256 | 512 | 1024 | 2048 = 128): string {
        return user.displayAvatarURL({
            format: 'png',
            dynamic: true,
            size
        });
    }

    static baseGuildHeader(guild: Guild, iconURL = DiscordEmbedFormatter.getIconURL(guild)): MessageEmbed {
        const isVip = guild.features.includes('VIP_REGIONS');

        return new MessageEmbed()
            .setAuthor(`${guild.name}${isVip ? ' ⭐' : ''}`, iconURL!, iconURL!)
            .setColor(guild.roles.highest.color);
    }

    static getIconURL(guild: Guild): string | null {
        return guild.iconURL({ format: 'png' });
    }
}
