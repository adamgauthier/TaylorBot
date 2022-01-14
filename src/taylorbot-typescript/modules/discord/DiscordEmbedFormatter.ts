import { Guild, MessageEmbed, User } from 'discord.js';

export class DiscordEmbedFormatter {
    static baseUserHeader(user: User, avatarURL = DiscordEmbedFormatter.getAvatarURL(user)): MessageEmbed {
        return new MessageEmbed()
            .setAuthor({ name: `${user.tag} ${(user.bot ? '🤖' : '')}`, iconURL: avatarURL, url: avatarURL });
    }

    static baseUserSuccessEmbed(user: User, avatarURL = DiscordEmbedFormatter.getAvatarURL(user)): MessageEmbed {
        return DiscordEmbedFormatter.baseUserHeader(user, avatarURL)
            .setColor('#43b581');
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
            .setAuthor({ name: `${guild.name}${isVip ? ' ⭐' : ''}`, iconURL: iconURL!, url: iconURL! })
            .setColor(guild.roles.highest.color);
    }

    static getIconURL(guild: Guild): string | null {
        return guild.iconURL({ format: 'png' });
    }
}
