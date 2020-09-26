import { SettableUserAttribute } from './SettableUserAttribute';
import { DatabaseDriver } from '../database/DatabaseDriver';
import { User } from 'discord.js';

export abstract class IntegerUserAttribute extends SettableUserAttribute {
    retrieve(database: DatabaseDriver, user: User): Promise<any> {
        return database.integerAttributes.get(this.id, user);
    }

    set(database: DatabaseDriver, user: User, value: any): Promise<Record<string, any>> {
        return database.integerAttributes.set(this.id, user, value.toString());
    }

    async clear(database: DatabaseDriver, user: User): Promise<void> {
        await database.integerAttributes.clear(this.id, user);
    }

    formatValue(attribute: Record<string, any>): string {
        return attribute.integer_value.toString();
    }
}
