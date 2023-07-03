import { Guild, EmbedBuilder, User } from 'discord.js';

export class DiscordEmbedFormatter {
    static baseUserHeader(user: User, avatarURL = DiscordEmbedFormatter.getAvatarURL(user)): EmbedBuilder {
        return new EmbedBuilder()
            .setAuthor({ name: `${user.discriminator === '0' ? user.username : user.tag} ${(user.bot ? '🤖' : '')}`, iconURL: avatarURL, url: avatarURL });
    }

    static baseUserSuccessEmbed(user: User, avatarURL = DiscordEmbedFormatter.getAvatarURL(user)): EmbedBuilder {
        return DiscordEmbedFormatter.baseUserHeader(user, avatarURL)
            .setColor('#43b581');
    }

    static getAvatarURL(user: User, size: 128 | 16 | 32 | 64 | 256 | 512 | 1024 | 2048 = 128): string {
        return user.displayAvatarURL({
            extension: 'png',
            size
        });
    }

    static baseGuildHeader(guild: Guild, iconURL = DiscordEmbedFormatter.getIconURL(guild)): EmbedBuilder {
        const isVip = guild.features.includes('VIP_REGIONS');

        return new EmbedBuilder()
            .setAuthor({ name: `${guild.name}${isVip ? ' ⭐' : ''}`, iconURL: iconURL!, url: iconURL! })
            .setColor(guild.roles.highest.color);
    }

    static getIconURL(guild: Guild): string | null {
        return guild.iconURL({ extension: 'png' });
    }
}
