import { Attribute, AttributeParameters } from './Attribute';
import DiscordEmbedFormatter = require('../modules/DiscordEmbedFormatter.js');
import PageMessage = require('../modules/paging/PageMessage.js');
import MemberEmbedDescriptionPageMessage = require('../modules/paging/editors/MemberEmbedDescriptionPageMessage.js');
import ArrayUtil = require('../modules/ArrayUtil.js');
import { DatabaseDriver } from '../database/DatabaseDriver';
import { GuildMember, Guild, MessageEmbed, Message } from 'discord.js';
import CommandMessageContext = require('../commands/CommandMessageContext');
import { TaylorBotClient } from '../client/TaylorBotClient';
import { MemberAttributePresenter } from './MemberAttributePresenter';

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

    async getCommand(commandContext: CommandMessageContext, member: GuildMember): Promise<MessageEmbed> {
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

    async rankCommand({ message, client }: { message: Message; client: TaylorBotClient }, guild: Guild): Promise<PageMessage> {
        const members = await this.rank(client.master.database, guild, 100);

        const embed = DiscordEmbedFormatter
            .baseGuildHeader(guild)
            .setTitle(`Ranking of ${this.description}`);

        return new PageMessage(
            client,
            message.author,
            ArrayUtil.chunk(members, 10),
            new MemberEmbedDescriptionPageMessage(
                embed,
                client.master.database,
                guild,
                (member: GuildMember, attribute: any) => this.presenter.presentRankEntry(member, attribute)
            )
        );
    }

    listCommand(commandContext: CommandMessageContext, guild: Guild): Promise<PageMessage> {
        throw new Error(`Method not implemented.`);
    }
}
