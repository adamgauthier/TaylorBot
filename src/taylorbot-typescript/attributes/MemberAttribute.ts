import { Attribute, AttributeParameters } from './Attribute';
import { DiscordEmbedFormatter } from '../modules/discord/DiscordEmbedFormatter';
import { PageMessage } from '../modules/paging/PageMessage';
import { MemberEmbedDescriptionPageMessage } from '../modules/paging/editors/MemberEmbedDescriptionPageMessage';
import { ArrayUtil } from '../modules/util/ArrayUtil';
import { DatabaseDriver } from '../database/DatabaseDriver';
import { GuildMember, Guild, EmbedBuilder } from 'discord.js';
import { MemberAttributePresenter } from './MemberAttributePresenter';
import { CommandMessageContext } from '../commands/CommandMessageContext';
import { DeprecatedCommandPageMessage } from '../modules/paging/DeprecatedCommandPageMessage';

export type MemberAttributeParameters = Omit<AttributeParameters, 'list'> & { presenter: new (a: MemberAttribute) => MemberAttributePresenter; columnName: string };

export abstract class MemberAttribute extends Attribute {
    readonly presenter: MemberAttributePresenter;
    readonly columnName: string;

    constructor(options: MemberAttributeParameters) {
        super({ ...options, list: null });
        this.presenter = new options.presenter(this);
        this.columnName = options.columnName;
    }

    abstract retrieve(database: DatabaseDriver, member: GuildMember): Promise<any>;

    async getCommand(commandContext: CommandMessageContext, member: GuildMember): Promise<EmbedBuilder> {
        const attribute = await this.retrieve(commandContext.client.master.database, member);

        if (!attribute) {
            return DiscordEmbedFormatter
                .baseUserHeader(member.user)
                .setColor('#f04747')
                .setDescription(`${member.displayName}'s ${this.description} doesn't exist. 🚫`);
        }
        else {
            return this.presenter.present(commandContext, member, attribute);
        }
    }

    abstract rank(database: DatabaseDriver, guild: Guild, entries: number): Promise<any[]>;

    async rankCommand({ client, author }: CommandMessageContext, guild: Guild): Promise<PageMessage<any>> {
        const members = await this.rank(client.master.database, guild, 100);

        if (typeof members[0] == 'string') {
            return new DeprecatedCommandPageMessage(members[0]);
        }

        const embed = DiscordEmbedFormatter
            .baseGuildHeader(guild)
            .setTitle(`Ranking of ${this.description}`);

        return new PageMessage(
            client,
            author,
            ArrayUtil.chunk(members, 10),
            new MemberEmbedDescriptionPageMessage(
                embed,
                client.master.database,
                guild,
                (member: GuildMember, attribute: any) => this.presenter.presentRankEntry(member, attribute)
            )
        );
    }

    listCommand(commandContext: CommandMessageContext, guild: Guild): Promise<PageMessage<any>> {
        throw new Error(`Method not implemented.`);
    }
}
