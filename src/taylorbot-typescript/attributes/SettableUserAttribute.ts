import DiscordFormatter = require('../modules/DiscordFormatter.js');
import DiscordEmbedFormatter = require('../modules/DiscordEmbedFormatter.js');
import PageMessage = require('../modules/paging/PageMessage.js');
import MemberEmbedDescriptionPageMessage = require('../modules/paging/editors/MemberEmbedDescriptionPageMessage.js');
import ArrayUtil = require('../modules/ArrayUtil.js');
import { UserAttribute, UserAttributeParameters } from './UserAttribute.js';
import { Guild, GuildMember, User, MessageEmbed } from 'discord.js';
import { DatabaseDriver } from '../database/DatabaseDriver.js';
import { CommandMessageContext } from '../commands/CommandMessageContext';

export type SettableUserAttributeParameters = Omit<UserAttributeParameters, 'canSet'> & { value: { label: string; type: string; example: string; maxDailySetCount?: number } };

export abstract class SettableUserAttribute extends UserAttribute {
    readonly value: { label: string; type: string; example: string; maxDailySetCount?: number };
    constructor(options: SettableUserAttributeParameters) {
        super({ ...options, canSet: true });
        this.value = options.value;
    }

    abstract set(database: DatabaseDriver, user: User, value: any): Promise<Record<string, any>>;

    abstract clear(database: DatabaseDriver, user: User): Promise<void>;

    async setCommand({ message, client }: CommandMessageContext, value: any): Promise<MessageEmbed> {
        const { author } = message;
        if (author === null)
            throw new Error(`Author can't be null.`);
        const attribute = await this.set(client.master.database, author, value);

        return DiscordEmbedFormatter
            .baseUserHeader(author)
            .setColor('#43b581')
            .setDescription(`Your ${this.description} has been set to '${this.formatValue(attribute)}'. ✅`);
    }

    async clearCommand({ message, client }: CommandMessageContext): Promise<MessageEmbed> {
        const { author } = message;
        if (author === null)
            throw new Error(`Author can't be null.`);
        await this.clear(client.master.database, author);

        return DiscordEmbedFormatter
            .baseUserEmbed(author)
            .setDescription(`Your ${this.description} has been cleared. ✅`);
    }

    async listCommand({ client, message }: CommandMessageContext, guild: Guild): Promise<PageMessage> {
        if (this.list === null)
            throw new Error(`Can't list this attribute.`);

        const attributes = await this.list(client.master.database, guild, 100);

        const embed = DiscordEmbedFormatter
            .baseGuildHeader(guild)
            .setTitle(`List of ${this.description}`);

        return new PageMessage(
            client,
            message.author,
            ArrayUtil.chunk(attributes, 20),
            new MemberEmbedDescriptionPageMessage(
                embed,
                client.master.database,
                guild,
                (member: GuildMember, attribute: any) => `${DiscordFormatter.escapeDiscordMarkdown(member.user.username)} - ${this.formatValue(attribute)}`
            )
        );
    }
}
