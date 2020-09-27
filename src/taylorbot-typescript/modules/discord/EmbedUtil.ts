import { MessageEmbed } from 'discord.js';

export class EmbedUtil {
    static success(text: string): MessageEmbed {
        return new MessageEmbed()
            .setColor('#43b581')
            .setDescription(text);
    }

    static error(text: string): MessageEmbed {
        return new MessageEmbed()
            .setColor('#f04747')
            .setDescription(text);
    }
}
