import { Format } from '../modules/discord/DiscordFormatter';
import { DiscordEmbedFormatter } from '../modules/discord/DiscordEmbedFormatter';
import { PageMessage } from '../modules/paging/PageMessage';
import { MemberEmbedDescriptionPageMessage } from '../modules/paging/editors/MemberEmbedDescriptionPageMessage';
import { ArrayUtil } from '../modules/util/ArrayUtil';
import { UserAttribute, UserAttributeParameters } from './UserAttribute.js';
import { Guild, GuildMember, User, EmbedBuilder } from 'discord.js';
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

    async setCommand({ author, client }: CommandMessageContext, value: any): Promise<EmbedBuilder> {
        const attribute = await this.set(client.master.database, author, value);

        return DiscordEmbedFormatter
            .baseUserHeader(author)
            .setColor('#43b581')
            .setDescription(`Your ${this.description} has been set to '${this.formatValue(attribute)}'. ✅`);
    }

    async clearCommand({ author, client }: CommandMessageContext): Promise<EmbedBuilder> {
        await this.clear(client.master.database, author);

        return DiscordEmbedFormatter
            .baseUserSuccessEmbed(author)
            .setDescription(`Your ${this.description} has been cleared. ✅`);
    }

    async listCommand({ client, author }: CommandMessageContext, guild: Guild): Promise<PageMessage<any>> {
        if (this.list === null)
            throw new Error(`Can't list this attribute.`);

        const attributes = await this.list(client.master.database, guild, 100);

        const embed = DiscordEmbedFormatter
            .baseGuildHeader(guild)
            .setTitle(`List of ${this.description}`);

        return new PageMessage(
            client,
            author,
            ArrayUtil.chunk(attributes, 20),
            new MemberEmbedDescriptionPageMessage(
                embed,
                client.master.database,
                guild,
                (member: GuildMember, attribute: any) => `${Format.escapeDiscordMarkdown(member.user.username)} - ${this.formatValue(attribute)}`
            )
        );
    }
}
