import DiscordFormatter = require('../modules/DiscordFormatter.js');
import { SettableUserAttribute, SettableUserAttributeParameters } from './SettableUserAttribute.js';
import { DatabaseDriver } from '../database/DatabaseDriver.js';
import { User } from 'discord.js';

export type TextUserAttributeParameters = Omit<SettableUserAttributeParameters, 'list'> & { canList: boolean };

export abstract class TextUserAttribute extends SettableUserAttribute {
    constructor(options: TextUserAttributeParameters) {
        super({
            ...options,
            list: options.canList ?
                ((database, guild, entries) => database.textAttributes.listInGuild(this.id, guild, entries)) :
                null
        });
    }

    retrieve(database: DatabaseDriver, user: User): Promise<any> {
        return database.textAttributes.get(this.id, user);
    }

    set(database: DatabaseDriver, user: User, value: any): Promise<Record<string, any>> {
        return database.textAttributes.set(this.id, user, value.toString());
    }

    clear(database: DatabaseDriver, user: User): Promise<void> {
        return database.textAttributes.clear(this.id, user);
    }

    formatValue(attribute: Record<string, any>): string {
        return DiscordFormatter.escapeDiscordMarkdown(attribute.attribute_value);
    }
}
