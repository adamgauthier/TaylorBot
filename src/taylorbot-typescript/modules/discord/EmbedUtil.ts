import { EmbedBuilder } from 'discord.js';

export class EmbedUtil {
    static success(text: string): EmbedBuilder {
        return new EmbedBuilder()
            .setColor('#43b581')
            .setDescription(text);
    }

    static error(text: string): EmbedBuilder {
        return new EmbedBuilder()
            .setColor('#f04747')
            .setDescription(text);
    }
}
