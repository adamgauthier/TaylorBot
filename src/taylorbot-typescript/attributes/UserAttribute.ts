import { Attribute, AttributeParameters } from './Attribute';
import { DiscordEmbedFormatter } from '../modules/discord/DiscordEmbedFormatter';
import { DatabaseDriver } from '../database/DatabaseDriver';
import { User, MessageEmbed, Guild } from 'discord.js';
import { UserAttributePresenter } from './UserAttributePresenter';
import { PageMessage } from '../modules/paging/PageMessage';
import { CommandMessageContext } from '../commands/CommandMessageContext';

export type UserAttributeParameters = AttributeParameters & { presenter: new (a: UserAttribute) => UserAttributePresenter; canSet: boolean };

export abstract class UserAttribute extends Attribute {
    readonly canSet: boolean;
    readonly presenter: UserAttributePresenter;
    constructor(options: UserAttributeParameters) {
        super(options);
        this.canSet = options.canSet;
        this.presenter = new options.presenter(this);
    }

    abstract retrieve(database: DatabaseDriver, user: User): Promise<any>;

    abstract formatValue(attribute: Record<string, any>): string;

    async getCommand(commandContext: CommandMessageContext, user: User): Promise<MessageEmbed> {
        const attribute = await this.retrieve(commandContext.client.master.database, user);

        if (!attribute) {
            const { client, messageContext } = commandContext;
            const embed = DiscordEmbedFormatter.baseUserHeader(user).setColor('#f04747');
            const setCommand = client.master.registry.commands.resolve(`set${this.id}`);

            if (this.canSet && setCommand) {
                const context = new CommandMessageContext(messageContext, setCommand);
                embed.setDescription([
                    `${user.username}'s ${this.description} is not set. 🚫`,
                    `They can use the \`${context.usage()}\` command to set it.`
                ].join('\n'));
            }
            else {
                embed.setDescription(`${user.username}'s ${this.description} doesn't exist. 🚫`);
            }

            return embed;
        }
        else {
            return this.presenter.present(commandContext, user, attribute);
        }
    }

    listCommand(commandContext: CommandMessageContext, guild: Guild): Promise<PageMessage<any>> {
        throw new Error(`Method not implemented.`);
    }
}
