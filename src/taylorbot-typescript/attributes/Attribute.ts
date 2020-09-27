import { DatabaseDriver } from '../database/DatabaseDriver';
import { Guild } from 'discord.js';
import { PageMessage } from '../modules/paging/PageMessage';
import { CommandMessageContext } from '../commands/CommandMessageContext';

export type AttributeParameters = { id: string; aliases: string[]; description: string; list: ((database: DatabaseDriver, guild: Guild, entries: number) => Promise<any>) | null };

export abstract class Attribute {
    readonly id: string;
    readonly aliases: string[];
    readonly description: string;
    readonly list: ((database: DatabaseDriver, guild: Guild, entries: number) => Promise<any>) | null;

    constructor({ id, aliases, description, list }: AttributeParameters) {
        this.id = id;
        this.aliases = aliases;
        this.description = description;
        this.list = list;
    }

    abstract listCommand(commandContext: CommandMessageContext, guild: Guild): Promise<PageMessage<any>>;
}
